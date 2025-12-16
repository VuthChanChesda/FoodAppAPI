using FoodAppAPI.Data;
using FoodAppAPI.Dtos;
using FoodAppAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FoodAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {
        //Entity Framework database context(gateway to the database)
        private readonly foodAppContext _context;

        // Constructor of the UserController
        // ASP.NET Core automatically calls this when creating the controller
        public UserController(foodAppContext context)
        {
            _context = context; 
        }

        //check if the user is admin by checking the role claim in the JWT token
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var userIdFromToken = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            if (id != userIdFromToken && !User.IsInRole("Admin"))
                return Forbid(); // 403 if not admin and not the same user

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }


        [Authorize]
        [AuthorizeOwner(typeof(User), "UserId")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, UpdateUserDto updatedUser)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(); // Return 404 if the user is not found
            }
            existingUser.Username = updatedUser.Username;
            existingUser.Email = updatedUser.Email;
            await _context.SaveChangesAsync();
            return NoContent(); // Return 204 No Content to indicate successful update

        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            //get the userId from the JWT claims instead of passing it from the url
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
 