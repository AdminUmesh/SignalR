using Microsoft.AspNetCore.SignalR;

namespace SignalRDemo.Hubs
{
    public class ChatHub : Hub
    {
        // Send to all clients
        public async Task SendToAll(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // Send to everyone except sender
        public async Task SendToOthers(string user, string message)
        {
            await Clients.Others.SendAsync("ReceiveMessage", user, message);
        }

        // Send to specific user
        public async Task SendToUser(string connectionId, string message)
        {
            await Clients.Client(connectionId)
                         .SendAsync("ReceivePrivateMessage", message);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }
    }
}