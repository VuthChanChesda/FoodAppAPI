using FoodAppAPI.Data;
using FoodAppAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace FoodAppAPI.Helpers
{
    public class ExpiredItemWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ExpiredItemWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckExpiredItemsAsync();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // every 5 minutes
            }
        }

        private async Task CheckExpiredItemsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<foodAppContext>();

            var now = DateTime.UtcNow;

            var expiredItems = await context.Items
                .Where(i =>
                    i.ExpiryDate != null &&
                    i.ExpiryDate <= now &&
                    !i.IsExpiredProcessed &&
                    i.Quantity > 0
                )
                .ToListAsync();

            foreach (var item in expiredItems)
            {
                var wasteLog = new WasteLog
                {
                    ItemId = item.ItemId,
                    QuantityWasted = item.Quantity,
                    Reason = "Expired",
                    DateWasted = now,
                    UserId = item.UserId
                };

                context.WasteLogs.Add(wasteLog);

                item.Quantity = 0;
                item.IsExpiredProcessed = true;
            }

            await context.SaveChangesAsync();
        }
    }

}
