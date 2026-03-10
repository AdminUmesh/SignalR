using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using SignalRDemo.Hubs;

namespace SignalRDemo.Controllers
{
    [ApiController]
    [Route("api/message")]
    public class MessageController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<ChatHub> _hub;

        public MessageController(IConfiguration config, IHubContext<ChatHub> hub)
        {
            _config = config;
            _hub = hub;
        }

        [HttpPost]
        public async Task<IActionResult> Send(string text)
        {
            var connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                "INSERT INTO Messages(Text) VALUES(@text)", conn);

            cmd.Parameters.AddWithValue("@text", text);

            await cmd.ExecuteNonQueryAsync();

            await _hub.Clients.All.SendAsync("ReceiveMessage", "API", text);

            return Ok();
        }
    }
}