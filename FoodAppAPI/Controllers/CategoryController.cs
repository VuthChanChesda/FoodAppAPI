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
                UserId = categorydto.UserId
            };
            _context.Categories.Add(categoryEntity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategories), new { id = categoryEntity.CategoryId }, categoryEntity);
        }

    }
}
