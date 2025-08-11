using Microsoft.AspNetCore.SignalR;

namespace SignalR.Hubs
{
    public class NotificationHub : Hub
    {
        // Called from client optionally
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
