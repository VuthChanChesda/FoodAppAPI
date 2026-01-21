using FoodAppAPI.Data;
using FoodAppAPI.Dtos;
using FoodAppAPI.Helpers;
using FoodAppAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace FoodAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly foodAppContext _context;
        private readonly PhotoHelper _photoHelper;
        private readonly OpenRouterService _openRouterService;
        private readonly HuggingFaceService _huggingFaceService;
        private readonly GeminiAiService _geminiAiService;

        public ItemController(foodAppContext context, PhotoHelper photoHelper, OpenRouterService openRouterService, HuggingFaceService huggingFaceService, GeminiAiService geminiAiService)
        {
            _context = context;
            _photoHelper = photoHelper;
            _openRouterService = openRouterService;
            _huggingFaceService = huggingFaceService;
            _geminiAiService = geminiAiService;
        }

        private static double CalculateExpiryProgress(DateTime? added, DateTime? expiry)
        {
            if (expiry == null)
                return 0;

            var addedDate = added ?? DateTime.UtcNow;
            var total = (expiry.Value - addedDate).TotalSeconds;
            if (total <= 0)
                return 100;

            var elapsed = (DateTime.UtcNow - addedDate).TotalSeconds;
            var progress = elapsed / total;

            return Math.Clamp(progress * 100, 0, 100);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var items = await _context.Items
                .Include(i => i.Category) 
                .Where(i => i.UserId == userId)
                .Select(i => new ItemResponseDto
                {
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    ImageUrl = i.ImageUrl,
                    Quantity = i.Quantity,
                    AddedDate = i.AddedDate,
                    ExpiryDate = i.ExpiryDate,
                    IsShoppingList = i.IsShoppingList,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category != null ? i.Category.CategoryName : "Uncategorized",
                    ExpiryProgress = CalculateExpiryProgress(i.AddedDate, i.ExpiryDate)
                })
                .ToListAsync();

            return Ok(items);
        }

        [Authorize, HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == id && i.UserId == userId);
            if (item == null)
                return NotFound();
            return Ok(new
            {
                item.ItemId,
                item.ItemName,
                item.Quantity,
                item.ImageUrl,
                item.CategoryId,
                item.IsShoppingList,
                item.AddedDate,
                item.ExpiryDate,
                item.IsExpiredProcessed
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromForm] ItemDto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if the Category exists and belongs to the user
            var category = await _context.Categories.FindAsync(item.CategoryId);
            if (category == null)
                return BadRequest($"Category with Id {item.CategoryId} does not exist.");

            if (category.UserId != userId)
                return Forbid(); // 403 Forbidden

            string? imageUrl = null;
            string? imagePublicId = null;

            if (item.Image != null)
            {
                var uploadResult = await _photoHelper.UploadImageAsync(item.Image);
                if (uploadResult?.Error != null)
                    return BadRequest(uploadResult.Error.Message);

                imageUrl = uploadResult.SecureUrl?.ToString();
                imagePublicId = uploadResult.PublicId;
            }

            var itemEntity = new Item
            {
                ItemName = item.ItemName,
                ImageUrl = imageUrl,
                PublicId = imagePublicId,
                CategoryId = item.CategoryId,
                Quantity = item.Quantity,
                IsShoppingList = item.IsShoppingList,
                AddedDate = item.AddedDate ?? DateTime.UtcNow,
                ExpiryDate = item.ExpiryDate,
                UserId = userId,
                IsExpiredProcessed = false
            };

            _context.Items.Add(itemEntity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }

            // Return the entity with actual database values (including ItemId and uploaded image info)
            var response = new
            {
                itemEntity.ItemId,
                itemEntity.ItemName,
                itemEntity.ImageUrl,
                itemEntity.PublicId,
                itemEntity.CategoryId,
                itemEntity.Quantity,
                itemEntity.IsShoppingList,
                itemEntity.AddedDate,
                itemEntity.ExpiryDate,
                itemEntity.UserId
                //itemEntiy.ExpiryProgress = CalculateExpiryProgress(itemEntity.AddedDate, itemEntity.ExpiryDate)
            };

            return CreatedAtAction(nameof(GetItem), new { id = itemEntity.ItemId }, response);
        }


        [Authorize]
        [AuthorizeOwner(typeof(Item), "UserId")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromForm] ItemDto updatedItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingItem = await _context.Items.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound(); // Return 404 if the item is not found
            }

            var category = await _context.Categories.FindAsync(updatedItem.CategoryId);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (category == null)
                return BadRequest($"Category with Id {updatedItem.CategoryId} does not exist.");

            // Ensure the category belongs to the current user
            if (category.UserId != userId)
                return Forbid(); // 403 Forbidden\\

            if (!string.IsNullOrEmpty(existingItem.PublicId) && updatedItem.Image == null)
            {
                // User wants to remove the existing image
                await _photoHelper.DeleteImageAsync(existingItem.PublicId);
                existingItem.ImageUrl = null;
                existingItem.PublicId = null;
            }


            if (updatedItem.Image != null)
            {
                // Delete the old image from Cloudinary since we are replacing it
                if (!string.IsNullOrEmpty(existingItem.PublicId))
                {
                    await _photoHelper.DeleteImageAsync(existingItem.PublicId);
                }

                //  Upload the new image
                var uploadResult = await _photoHelper.UploadImageAsync(updatedItem.Image);
                if (uploadResult?.Error != null)
                    return BadRequest(uploadResult.Error.Message);

                //  Update the database with the new URL and PublicId
                existingItem.ImageUrl = uploadResult.SecureUrl?.ToString();
                existingItem.PublicId = uploadResult.PublicId;
            }

            existingItem.ItemName = updatedItem.ItemName;
            existingItem.CategoryId = updatedItem.CategoryId;
            existingItem.Quantity = updatedItem.Quantity;
            existingItem.IsShoppingList = updatedItem.IsShoppingList;
            existingItem.AddedDate = updatedItem.AddedDate ?? existingItem.AddedDate;
            existingItem.ExpiryDate = updatedItem.ExpiryDate;
            existingItem.IsExpiredProcessed = updatedItem.IsExpiredProcessed;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                //Optional: catch other database exceptions
                return BadRequest(ex.Message);
            }
            return NoContent(); // Return 204 No Content on successful update
        }

        [Authorize]
        [AuthorizeOwner(typeof(Item), "UserId")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var existingItem = await _context.Items.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound(); 
            }
            if (!string.IsNullOrEmpty(existingItem.PublicId))
            {
                await _photoHelper.DeleteImageAsync(existingItem.PublicId);
            }
            _context.Items.Remove(existingItem);
            await _context.SaveChangesAsync();
            return NoContent(); 
        }


    [Authorize] 
    [HttpPatch("toggle-location")]
    public async Task<IActionResult> ToggleLocation([FromBody] ToggleLocationDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var item = await _context.Items.FindAsync(request.ItemId);

        if (item == null)
        {
            return NotFound(new { message = "Item not found" });
        }

        //SECURITY CHECK: Compare the item's OwnerId with the JWT's UserId
        if (item.UserId != currentUserId)
        {
            return Forbid(); // Return 403 Forbidden if they don't own the item
        }

        // Toggle the location
        item.IsShoppingList = !item.IsShoppingList;

        item.Quantity = request.Amount >= 0 ? request.Amount : item.Quantity;

        if (!item.IsShoppingList)
        {
            item.AddedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [Authorize]
    [HttpPost("consume")]
    public async Task<IActionResult> ConsumeItem(ConsumeItemDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId && i.UserId == userId);

        if (item == null)
            return NotFound("Item not found or access denied");

        var consumption = new Consumption
        {
            ItemId = item.ItemId,
            UserId = userId,
            AmountConsumed = request.ConsumptionAmount,
            ConsumedAt = DateTime.UtcNow,
            CategoryId = item.CategoryId
        };

        item.Quantity -= request.ConsumptionAmount;

        if (item.Quantity <= 0)
        {
            item.Quantity = 0;
            item.IsShoppingList = true;
        }

        // Save both changes to the database
        _context.Consumption.Add(consumption);
        _context.Items.Update(item);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            itemId = item.ItemId,
            itemName = item.ItemName,
            quantity = item.Quantity,
            isShoppingList = item.IsShoppingList
        });
    }

        [Authorize]
        [HttpPost("generate-recipe-from-selection")]
        public async Task<IActionResult> GenerateRecipeFromSelection([FromBody] List<int> selectedItemIds)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int currentUserId)) return Unauthorized();

            if (selectedItemIds == null || !selectedItemIds.Any())
            {
                return BadRequest("Please select at least one ingredient.");
            }

            // Fetch only the names of the selected items that belong to this user
            var ingredients = await _context.Items
                .Where(i => selectedItemIds.Contains(i.ItemId) && i.UserId == currentUserId)
                .Select(i => i.ItemName)
                .ToListAsync();

            if (!ingredients.Any())
            {
                return BadRequest("None of the selected items were found in your pantry.");
            }

            // Call OpenRouter with the specific list
            //var recipeText = await _huggingFaceService.GetRecipeAsync(ingredients);
            var recipeText = await _geminiAiService.GetRecipeAsync(ingredients);


            return Ok(new { recipe = recipeText });
        }

        [HttpPost("search-nearby-markets")]
        public async Task<IActionResult> SearchNearbyMarkets([FromBody] string locationText)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int currentUserId)) return Unauthorized();

            var shoppingList = await _context.Items
                .Where(i => i.UserId == currentUserId && i.IsShoppingList)
                .Select(i => i.ItemName)
                .ToListAsync();

            if (!shoppingList.Any())
            {
                return BadRequest("Your shopping list is empty. Add items first!");
            }

            var marketAdvice = await _geminiAiService.FindNearbyMarketsAsync(locationText, shoppingList);

            return Ok(new { advice = marketAdvice });
        }


    }


}
