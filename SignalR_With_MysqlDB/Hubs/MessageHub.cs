using Microsoft.AspNetCore.SignalR;

namespace SignalR.Hubs
{
    public class MessageHub : Hub
    {
        // No extra methods — we’ll just use Clients.All.SendAsync from server
    }
}
