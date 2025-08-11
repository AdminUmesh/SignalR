using Microsoft.AspNetCore.SignalR;

namespace SignalR_Without_DB.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string message)
        {
            // Sends the message to ALL connected clients
            await Clients.All.SendAsync("ReceiveMessage", $"Server: {message}");
        }
    }
}

