using Microsoft.AspNetCore.SignalR;

namespace SimpleChatUI.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveUserMessage", user, message);
    }
}
