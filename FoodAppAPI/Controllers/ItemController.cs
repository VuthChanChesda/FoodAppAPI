using FoodAppAPI.Data;
using FoodAppAPI.Dtos;
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

        public ItemController(foodAppContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var userId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
            var items = await _context.Items
                .Where(i => i.UserId == userId)
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
            return Ok(item);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateItem(ItemDto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            //Check if the Category exists and belongs to the user
            var category = await _context.Categories.FindAsync(item.CategoryId);
            if (category == null)
                return BadRequest($"Category with Id {item.CategoryId} does not exist.");

            // Ensure the category belongs to the current user
            if (category.UserId != userId)
                return Forbid(); // 403 Forbidden


            var itemEntity = new Item
            {
                ItemName = item.ItemName,
                CategoryId = item.CategoryId,
                Quantity = item.Quantity,
                IsShoppingList = item.IsShoppingList,
                //if user dont provide date set to current date
                AddedDate =  item.AddedDate ?? DateTime.UtcNow, 
                ExpiryDate = item.ExpiryDate,
                UserId = userId,
            };
            _context.Items.Add(itemEntity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                //Optional: catch other database exceptions
                return BadRequest(ex.Message);
            }
            return CreatedAtAction(nameof(GetItem), new { id = itemEntity.ItemId }, item);
        }

        [Authorize]
        [AuthorizeOwner(typeof(Item), "UserId")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, ItemDto updatedItem)
        {
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
                return Forbid(); // 403 Forbidden

            existingItem.ItemName = updatedItem.ItemName;
            existingItem.CategoryId = updatedItem.CategoryId;
            existingItem.Quantity = updatedItem.Quantity;
            existingItem.IsShoppingList = updatedItem.IsShoppingList;
            existingItem.AddedDate = updatedItem.AddedDate ?? existingItem.AddedDate;
            existingItem.ExpiryDate = updatedItem.ExpiryDate;
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
            _context.Items.Remove(existingItem);
            await _context.SaveChangesAsync();
            return NoContent(); 
        }


    }
}
