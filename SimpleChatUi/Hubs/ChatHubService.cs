using Microsoft.AspNetCore.SignalR.Client;

namespace SimpleChatUi.Hubs
{
    public class ChatHubService
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<ChatHubService> _logger;

        public event EventHandler<BotMessageReceivedEventArgs>? BotMessageReceived;

        public ChatHubService(ILogger<ChatHubService> logger)
        {
            _logger = logger;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:6217/chatHub")
                .Build();

            _hubConnection.On("ReceiveBotMessage", (Action<string, string>)((bot, answer) =>
            {
                OnBotMessageReceived(new BotMessageReceivedEventArgs(answer));
            }));
        }

        public async Task EnsureConnectionAsync()
        {
            while (_hubConnection.State != HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation($"(｡◕‿‿◕｡) Connected to {_hubConnection.ConnectionId}!");
                }
                catch (Exception)
                {
                    _logger.LogWarning($"*** Not Connected: {_hubConnection.State}");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }

        public async Task SendMessageToBotAsync(string user, string message)
        {
            if (_hubConnection is null)
                return;

            await EnsureConnectionAsync();

            try
            {
                // Call a method on the SignalR hub to send a message
                await _hubConnection.SendAsync("ReceiveUserPrompt", user, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to SignalR hub: {ex.Message}");
            }
        }

        private void OnBotMessageReceived(BotMessageReceivedEventArgs e)
        {
            BotMessageReceived?.Invoke(this, e);
        }

        public class BotMessageReceivedEventArgs(string answer)
        : EventArgs
        {
            public string Answer { get; } = answer;
        }
    }
}