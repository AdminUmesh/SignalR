using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using SignalRDemo.Hubs;

namespace SignalRDemo.Services
{
    public class DatabasePollingService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<ChatHub> _hub;
        private int _lastId = 0;

        public DatabasePollingService(
            IConfiguration config,
            IHubContext<ChatHub> hub)
        {
            _config = config;
            _hub = hub;
        }

        protected override async Task ExecuteAsync( 
            CancellationToken stoppingToken)
        {
            var connStr = _config.GetConnectionString("DefaultConnection");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var conn = new SqlConnection(connStr);

                await conn.OpenAsync(stoppingToken);

                var cmd = new SqlCommand(
                    "SELECT Id,Text FROM Messages WHERE Id>@id",
                    conn);

                cmd.Parameters.AddWithValue("@id", _lastId);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    string text = reader.GetString(1);

                    _lastId = id;

                    await _hub.Clients.All.SendAsync(
                        "ReceiveMessage",
                        "DB",
                        text);
                }

                await Task.Delay(2000);
            }
        }
    }
}   