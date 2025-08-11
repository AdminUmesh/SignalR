using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalR.Models;
using SignalR.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using SignalR.Hubs;

namespace SignalR.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<NotificationHub> _hubContext;

        public HomeController(IConfiguration config, IHubContext<NotificationHub> hubContext)
        {
            _config = config;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([FromForm] string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("INSERT INTO messages (text) VALUES (@text)", conn);
                cmd.Parameters.AddWithValue("@text", message);
                await cmd.ExecuteNonQueryAsync();

                // Immediately push to connected clients (server-originated)
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"[App] {message}");
            }

            return RedirectToAction("Index");
        }
    }
}
