using FoodAppAPI.Data;
using FoodAppAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly foodAppContext _context;
        public CategoryController(foodAppContext context)
        {
            _context = context;
        }

        //filter categories by userId from JWT token
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(categories);
        }

        [Authorize, HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
            if (category == null)
                return NotFound();
            return Ok(category);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryDTO categorydto)
        {
            //check model state validation base on the requirements in CategoryDTO
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var categoryEntity = new Category
            {
                CategoryName = categorydto.CategoryName,
                Emoji = categorydto.Emoji,
                UserId = userId
            };
            _context.Categories.Add(categoryEntity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategories), new { id = categoryEntity.CategoryId }, categoryEntity);
        }

        [Authorize]
        [AuthorizeOwner(typeof(Category), "UserId")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDTO NewCategory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var oldCategory = await _context.Categories.FindAsync(id);
            if (oldCategory == null)
                return NotFound();
            oldCategory.CategoryName = NewCategory.CategoryName;
            oldCategory.Emoji = NewCategory.Emoji;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [AuthorizeOwner(typeof(Category), "UserId")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
