# Using SignalR in ASP.NET Core (.NET)


------------------------------------------------------------------------

# 1. What is SignalR?

SignalR is a **real-time communication framework for ASP.NET Core**.

It allows a server to **push data to connected clients instantly**
without requiring clients to repeatedly request updates.

Traditional web apps:

Client → Request → Server → Response

SignalR:

Client ⇄ Persistent Connection ⇄ Server

The server can **push messages to clients at any time**.

------------------------------------------------------------------------

# 2. Common Use Cases

SignalR is used in:

-   Chat applications
-   Notification systems
-   Live dashboards
-   Trading systems
-   Multiplayer games
-   Collaborative editing (Google Docs style)
-   Monitoring systems

------------------------------------------------------------------------

# 3. Transport Mechanisms

SignalR automatically chooses the best transport.

Priority order:

1.  WebSockets (best performance)
2.  Server Sent Events
3.  Long Polling

Developers **do not manage these manually**.

------------------------------------------------------------------------

# 4. Core SignalR Components

  |Component     | Description|
  |--------------| ----------------------------------------|
  | Hub         |   Server-side endpoint for communication |
  | Client      |   Browser or app connected to hub   |
  |ConnectionId  | Unique id per connection |
  |UserId        | Identifier for authenticated user |
  |Groups        | Logical grouping of clients |
  |HubContext     |Send messages outside hub |

------------------------------------------------------------------------

# 5. Create New Project

Using CLI:

``` bash
dotnet new mvc -n SignalRDemo
cd SignalRDemo
```

Or in Visual Studio:

File → New → ASP.NET Core Web App (MVC)

------------------------------------------------------------------------

# 6. Install Required Packages

Install SignalR:

``` bash
dotnet add package Microsoft.AspNetCore.SignalR
```

Client JavaScript library will be loaded via CDN.

------------------------------------------------------------------------

# 7. Configure Program.cs

Open **Program.cs**.

Add SignalR service.

``` csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
```

This exposes a **SignalR endpoint**:

/chatHub

------------------------------------------------------------------------

# 8. Create a Hub

Create folder:

Hubs

Create file:

ChatHub.cs

``` csharp
using Microsoft.AspNetCore.SignalR;

namespace SignalRDemo.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendToAll(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendToOthers(string user, string message)
        {
            await Clients.Others.SendAsync("ReceiveMessage", user, message);
        }

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
```

------------------------------------------------------------------------

# 9. Understanding Hub Methods

SignalR hub methods are **invoked by clients**.

Example:

Client calls:

    connection.invoke("SendToAll", user, message)

Server method executes:

    SendToAll()

Server then pushes messages to clients.

------------------------------------------------------------------------

# 10. Create Client Page

Open:

Views/Home/Index.cshtml

Add:

``` html
<h2>SignalR Chat Demo</h2>

<div>
ConnectionId:
<span id="connectionId"></span>
</div>

<input id="user" placeholder="User" />
<input id="message" placeholder="Message" />

<button onclick="sendAll()">Send To All</button>
<button onclick="sendOthers()">Send To Others</button>

<hr>

<ul id="messages"></ul>

<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/signalr.min.js"></script>

<script>

const connection = new signalR.HubConnectionBuilder()
.withUrl("/chatHub")
.withAutomaticReconnect()
.build();

connection.start().then(() => {

document.getElementById("connectionId").innerText = connection.connectionId;

});

connection.on("ReceiveMessage", (user, message) => {

const li = document.createElement("li");

li.textContent = user + ": " + message;

document.getElementById("messages").appendChild(li);

});

function sendAll(){

const user = document.getElementById("user").value;
const message = document.getElementById("message").value;

connection.invoke("SendToAll", user, message);

}

function sendOthers(){

const user = document.getElementById("user").value;
const message = document.getElementById("message").value;

connection.invoke("SendToOthers", user, message);

}

</script>
```

------------------------------------------------------------------------

# 11. Testing the Application

Run:

    dotnet run

Open multiple browser tabs.

Test broadcasting:

    connection.invoke("SendToAll","User1","Hello Everyone")

All tabs receive message instantly.

------------------------------------------------------------------------

# 12. Sending Messages to Specific User

Instead of connectionId, production systems use **UserId**.

Example:

    Clients.User(userId)

This sends message to **all active connections of that user**.

------------------------------------------------------------------------

# 13. Groups (Chat Rooms)

Users can join groups.

``` csharp
await Groups.AddToGroupAsync(Context.ConnectionId,"room1");
```

Send message to group:

``` csharp
await Clients.Group("room1").SendAsync("ReceiveMessage",message);
```

------------------------------------------------------------------------

# 14. Sending Messages Outside Hub

Use **IHubContext**.

Example inside controller:

``` csharp
public class NotificationController : Controller
{
    private readonly IHubContext<ChatHub> _hub;

    public NotificationController(IHubContext<ChatHub> hub)
    {
        _hub = hub;
    }

    public async Task Send(string msg)
    {
        await _hub.Clients.All.SendAsync("ReceiveMessage","Server",msg);
    }
}
```

------------------------------------------------------------------------

# 15. Detecting Database Changes

Often used with:

-   Background services
-   Message queues
-   Database triggers

Example architecture:

Database change → BackgroundService → SignalR → Clients

------------------------------------------------------------------------

# 16. Scaling SignalR

When multiple servers exist:

Clients connect to different servers.

Use **Redis backplane**.

Architecture:

Clients ↓ Load Balancer ↓ SignalR Servers ↓ Redis ↓ Database

Redis synchronizes messages across servers.

------------------------------------------------------------------------

# 17. Security

Use authentication:

    [Authorize]
    public class ChatHub : Hub

User identity becomes available via:

    Context.UserIdentifier

------------------------------------------------------------------------

# 18. Best Practices

Use:

-   UserId instead of connectionId
-   Groups for chat rooms
-   Redis for scaling
-   Background services for DB events

Avoid:

-   excessive polling
-   sending large payloads frequently

------------------------------------------------------------------------

# 19. Real-World Example Flow

User A sends message.

Browser → SignalR Hub → Server

Server processes message.

Server → SignalR → All connected clients

Clients instantly update UI.

------------------------------------------------------------------------

# 20. Summary

SignalR enables:

-   Real-time communication
-   Broadcasting
-   Targeted messaging
-   Group messaging
-   Persistent connections

It is widely used in modern real-time systems.

------------------------------------------------------------------------

End of Deep Guide
