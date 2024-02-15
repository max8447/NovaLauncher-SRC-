using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace NovaLauncher.Models.WebSocket
{
    public class SignalRClient
    {
        private HubConnection _connection;
        private bool _connected;

        public event Action<string, string> OnMessageReceived;

        public async Task Connect(string serverUrl)
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(serverUrl)
                    .WithAutomaticReconnect()
                    .Build();

                _connection.Closed += async (error) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await Connect(serverUrl);
                };

                _connection.On<string, string>("ReceiveMessage", (user, message) =>
                {
                    OnMessageReceived?.Invoke(user, message);
                });

                await _connection.StartAsync();

                _connected = true;
            }
            catch (Exception ex)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await Connect(serverUrl);
            }
        }

        public async Task Disconnect()
        {
            if (_connected)
            {
                await _connection.StopAsync();
                _connected = false;
            }
        }
    }
}
