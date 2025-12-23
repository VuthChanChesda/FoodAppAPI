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
    public class WasteLogController : ControllerBase
    {
        private readonly foodAppContext _context;
        public WasteLogController(foodAppContext context)
        {
            _context = context;
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWasteLogs() { 

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var wasteLogs = await _context.WasteLogs
                .Where(w => w.UserId == userId)
                .ToListAsync();
            return Ok(wasteLogs);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWasteLog(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var wasteLog = await _context.WasteLogs
                .FirstOrDefaultAsync(w => w.WasteLogId == id && w.UserId == userId);
            if (wasteLog == null)
                return NotFound();
            return Ok(wasteLog);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GenerateWasteLog(WasteLogDto waste)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verify item exists and belongs to user
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == waste.ItemId && i.UserId == userId);

            if (item == null)
                return NotFound("Item not found or does not belong to you.");

            if (waste.QuantityWasted <= 0)
                return BadRequest("Invalid wasted quantity.");

            if (waste.QuantityWasted > item.Quantity)
                return BadRequest("Wasted quantity exceeds available item quantity.");

            // Reduce item quantity
            item.Quantity -= waste.QuantityWasted;

            // auto-delete item if quantity = 0
            if (item.Quantity == 0)
                _context.Items.Remove(item);

            var wasteLog = new WasteLog
            {
                ItemId = item.ItemId,
                QuantityWasted = waste.QuantityWasted,
                Reason = waste.Reason,
                DateWasted = waste.DateWasted ?? DateTime.UtcNow,
                UserId = userId
            };

            _context.WasteLogs.Add(wasteLog);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetWasteLog),
                new { id = wasteLog.WasteLogId },
                wasteLog
            );
        }


        [Authorize]
        [AuthorizeOwner(typeof(WasteLog), "UserId")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWasteLog(int id, WasteLogDto updatedWasteLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var existingWasteLog = await _context.WasteLogs.FindAsync(id);
            if (existingWasteLog == null || existingWasteLog.UserId != userId)
            {
                return NotFound();
            }
            // Verify item exists and belongs to user
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == updatedWasteLog.ItemId && i.UserId == userId);
            if (item == null)
                return NotFound("Item not found or does not belong to you.");
            existingWasteLog.ItemId = updatedWasteLog.ItemId;
            existingWasteLog.QuantityWasted = updatedWasteLog.QuantityWasted;
            existingWasteLog.Reason = updatedWasteLog.Reason;
            existingWasteLog.DateWasted = updatedWasteLog.DateWasted;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [AuthorizeOwner(typeof(WasteLog), "UserId")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWasteLog(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var existingWasteLog = await _context.WasteLogs.FindAsync(id);
            if (existingWasteLog == null || existingWasteLog.UserId != userId)
            {
                return NotFound();
            }
            _context.WasteLogs.Remove(existingWasteLog);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
