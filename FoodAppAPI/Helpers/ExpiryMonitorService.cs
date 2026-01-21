using FoodAppAPI.Data;
using Microsoft.AspNetCore.SignalR;
using System;

namespace FoodAppAPI.Helpers
{

    public class ExpiryMonitorService : BackgroundService
    {
        private readonly IHubContext<ExpiryHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;

        public ExpiryMonitorService(IHubContext<ExpiryHub> hubContext, IServiceProvider serviceProvider)
        {
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Access your Database Context safely inside a background worker
                    var dbContext = scope.ServiceProvider.GetRequiredService<foodAppContext>();

                    var now = DateTime.UtcNow;
                    var window = now.AddSeconds(-40); // Catch items expired in the last 40s

                    var newlyExpiredItems = dbContext.Items
                        .Where(i => i.ExpiryDate <= now && i.ExpiryDate >= window)
                        .ToList();

                    foreach (var item in newlyExpiredItems)
                    {
                        // PUSH: Notify all connected Flutter clients
                        await _hubContext.Clients.All.SendAsync("ReceiveExpiryAlert", item.ItemName, item.ItemId);
                        Console.WriteLine($"[SignalR] Alert sent for: {item.ItemName}");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
