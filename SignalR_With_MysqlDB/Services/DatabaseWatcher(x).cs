using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using SignalR.Hubs;
using SignalR.Hubs;

namespace SignalR.Services
{
    public class DatabaseWatcher : BackgroundService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration _config;
        private int _lastMaxId = 0;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);

        public DatabaseWatcher(IHubContext<NotificationHub> hubContext, IConfiguration config)
        {
            _hubContext = hubContext;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connStr = _config.GetConnectionString("DefaultConnection");

            // Initialize last max id on start
            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync(stoppingToken);
                using var cmd = new MySqlCommand("SELECT COALESCE(MAX(id), 0) FROM messages", conn);
                var res = await cmd.ExecuteScalarAsync(stoppingToken);
                _lastMaxId = Convert.ToInt32(res);
            }
            catch
            {
                _lastMaxId = 0;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var conn = new MySqlConnection(connStr);
                    await conn.OpenAsync(stoppingToken);

                    var cmd = new MySqlCommand("SELECT id, text, created_at FROM messages WHERE id > @lastId ORDER BY id ASC", conn);
                    cmd.Parameters.AddWithValue("@lastId", _lastMaxId);

                    using var reader = await cmd.ExecuteReaderAsync(stoppingToken);
                    var newMax = _lastMaxId;
                    while (await reader.ReadAsync(stoppingToken))
                    {
                        var id = reader.GetInt32(reader.GetOrdinal("id"));
                        var text = reader.GetString(reader.GetOrdinal("text"));
                        var createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

                        // Push to connected clients
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"[DB] {text} (id:{id})");

                        if (id > newMax) newMax = id;
                    }
                    _lastMaxId = newMax;
                }
                catch (Exception ex)
                {
                    // log or ignore for demo
                    Console.WriteLine($"DatabaseWatcher error: {ex.Message}");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
    }
}
