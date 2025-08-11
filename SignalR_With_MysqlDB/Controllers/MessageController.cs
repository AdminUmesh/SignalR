using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using SignalR.Hubs;
using SignalR.Hubs;

namespace SignalR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<MessageHub> _hubContext;

        public MessageController(IConfiguration config, IHubContext<MessageHub> hubContext)
        {
            _config = config;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] string text)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new MySqlConnection(connStr);
            await conn.OpenAsync();

            string insert = "INSERT INTO messages (text) VALUES (@text)";
            using var cmd = new MySqlCommand(insert, conn);
            cmd.Parameters.AddWithValue("@text", text);
            await cmd.ExecuteNonQueryAsync();

            // Push instantly to clients
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            return Ok();
        }
    }
}
