using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using SignalR.Hubs;
using SignalR.Models;
using SignalR.Hubs;
using SignalR.Models;
using System.Data;

namespace SignalR.Services
{
    public class MessagePollingService : BackgroundService
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly IConfiguration _config;
        private int _lastSeenId = 0;

        public MessagePollingService(IHubContext<MessageHub> hubContext, IConfiguration config)
        {
            _hubContext = hubContext;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var conn = new MySqlConnection(connStr);
                    await conn.OpenAsync(stoppingToken);

                    string query = "SELECT id, text, created_at FROM messages WHERE id > @lastId ORDER BY id ASC";
                    using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@lastId", _lastSeenId);

                    using var reader = await cmd.ExecuteReaderAsync(stoppingToken);
                    while (await reader.ReadAsync(stoppingToken))
                    {
                        var msg = new Message
                        {
                            Id = reader.GetInt32("id"),
                            Text = reader.GetString("text"),
                            CreatedAt = reader.GetDateTime("created_at")
                        };

                        _lastSeenId = msg.Id; // update tracker

                        // Push to all SignalR clients
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", msg.Text, msg.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Polling error: " + ex.Message);
                }

                await Task.Delay(2000, stoppingToken); // poll every 2s
            }
        }
    }
}
