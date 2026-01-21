using FoodAppAPI.Data;
using FoodAppAPI.Dtos.InsightDataDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AnalyticsController : ControllerBase
    {
        private readonly foodAppContext _context;
        public AnalyticsController(foodAppContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ItemInsightDto>> GetPantryAnalysis()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var now = DateTime.UtcNow;

            // Orchestrating smaller functions
            var wasteStats = await GetWasteStats(userId, now);
            var consumptionStats = await GetConsumptionStats(userId, now);
            var weeklyTrend = await GetWeeklyConsumptionTrend(userId, now);
            var categoryWaste = await GetWasteByCategory(userId);

            return Ok(new ItemInsightDto
            {
                MonthlyWasteCount = wasteStats.CurrentCount,
                WasteChangePercentage = wasteStats.ChangePercentage,
                MonthlyConsumedCount = consumptionStats.CurrentCount,
                ConsumedChangePercentage = consumptionStats.ChangePercentage,
                MostConsumedItem = await GetMostConsumedItem(userId),
                WeeklyConsumption = weeklyTrend,
                WasteByCategory = categoryWaste
            });
        }

        private async Task<(int CurrentCount, double ChangePercentage)> GetWasteStats(int userId, DateTime now)
        {
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);

            var current = await _context.WasteLogs
                .CountAsync(w => w.UserId == userId && w.DateWasted >= startOfCurrentMonth);
            var previous = await _context.WasteLogs
                .CountAsync(w => w.UserId == userId && w.DateWasted >= startOfLastMonth && w.DateWasted < startOfCurrentMonth);

            return (current, CalculatePercentageChange(current, previous));
        }

        private async Task<List<WeeklyTrendDto>> GetWeeklyConsumptionTrend(int userId, DateTime now)
        {
            var last7Days = Enumerable.Range(0, 7).Select(i => now.AddDays(-i).Date).Reverse();

            var data = await _context.Consumption
                .Where(c => c.UserId == userId && c.ConsumedAt >= now.AddDays(-7))
                .GroupBy(c => c.ConsumedAt.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync();

            return last7Days.Select(date => new WeeklyTrendDto
            {
                Day = date.ToString("ddd"),
                Value = data.FirstOrDefault(d => d.Day == date)?.Count ?? 0
            }).ToList();
        }

        private async Task<List<CategoryWasteDto>> GetWasteByCategory(int userId)
        {
            // Get total waste count for this user
            var totalWaste = await _context.WasteLogs
                .CountAsync(w => w.UserId == userId);

            if (totalWaste == 0) return new List<CategoryWasteDto>();

            // Join WasteLogs with Items to reach the Category data
            return await _context.WasteLogs
                .Where(w => w.UserId == userId)
                .Join(_context.Items,
                    waste => waste.ItemId,
                    item => item.ItemId,
                    (waste, item) => new { item.Category.CategoryName }) // Navigating to Category
                .GroupBy(x => x.CategoryName)
                .Select(g => new CategoryWasteDto
                {
                    CategoryName = g.Key ?? "Uncategorized",
                    Percentage = Math.Round(((double)g.Count() / totalWaste) * 100, 1)
                })
                .ToListAsync();
        }

        private async Task<(int CurrentCount, double ChangePercentage)> GetConsumptionStats(int userId, DateTime now)
        {
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);

            // Count items consumed this month
            var current = await _context.Consumption
                .CountAsync(c => c.UserId == userId && c.ConsumedAt >= startOfCurrentMonth);

            // Count items consumed last month
            var previous = await _context.Consumption
                .CountAsync(c => c.UserId == userId && c.ConsumedAt >= startOfLastMonth && c.ConsumedAt < startOfCurrentMonth);

            return (current, CalculatePercentageChange(current, previous));
        }

        private async Task<MostConsumedItemDto> GetMostConsumedItem(int userId)
        {
            var topItem = await _context.Consumption
                .Where(c => c.UserId == userId)
                //  Join Consumption -> Items
                .Join(_context.Items,
                    consumption => consumption.ItemId,
                    item => item.ItemId,
                    (consumption, item) => new { item.ItemName, item.CategoryId })
                // Join Result -> Categories
                .Join(_context.Categories,
                    combined => combined.CategoryId,
                    category => category.CategoryId,
                    (combined, category) => new
                    {
                        combined.ItemName,
                        category.CategoryName
                    })
                //  Group by both to keep data consistent
                .GroupBy(x => new { x.ItemName, x.CategoryName })
                .OrderByDescending(g => g.Count())
                .Select(g => new MostConsumedItemDto
                {
                    Name = g.Key.ItemName,
                    Category = g.Key.CategoryName // Now you have the string name!
                })
                .FirstOrDefaultAsync();

            return topItem ?? new MostConsumedItemDto { Name = "None", Category = "N/A" };
        }

        private double CalculatePercentageChange(int current, int previous)
        {
            if (previous == 0) return 0;
            return Math.Round(((double)(current - previous) / previous) * 100, 1);
        }


    }
}
