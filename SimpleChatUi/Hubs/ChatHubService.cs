using Microsoft.AspNetCore.SignalR.Client;

namespace SimpleChatUi.Hubs;

public class ChatHubService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<ChatHubService> _logger;

    //public delegate void BotMessageReceivedEventHandler(object? sender, BotAnswerReceivedEventArgs e);
    public event Action<object?, BotAnswerReceivedEventArgs>? BotMessageReceived;

    public ChatHubService(ILogger<ChatHubService> logger)
    {
        _logger = logger;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:6217/chatHub")
            .Build();

        _hubConnection.On("ReceiveBotMessage", (Action<string, string, bool>)((bot, answer, isDone) =>
        {
            OnBotMessageReceived(new BotAnswerReceivedEventArgs(bot, answer, isDone));
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

    private void OnBotMessageReceived(BotAnswerReceivedEventArgs e)
    {
        BotMessageReceived?.Invoke(this, e);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}