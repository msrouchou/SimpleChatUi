namespace SimpleChatUi.Hubs;

public sealed class BotAnswerReceivedEventArgs(string user, string answer, bool isDone) : EventArgs
{
    public string TargetUser { get; set; } = user;
    public string Answer { get; set; } = answer;
    public bool IsDone { get; set; } = isDone;
}
