using Microsoft.AspNetCore.SignalR;

namespace FoodAppAPI.Helpers
{
        public class ExpiryHub : Hub
        {
            public override async Task OnConnectedAsync()
            {
                Console.WriteLine($"--> Client Connected: {Context.ConnectionId}");
                await base.OnConnectedAsync();
            }
        }
    
}
