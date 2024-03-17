namespace SimpleChatUi.Components;

public class EchoBot
{
    public static List<string> GetAnswer(string message, int chunkSize)
    {
        var chunks = new List<string>();
        for (int i = 0; i < message.Length; i += chunkSize)
        {
            if (i + chunkSize > message.Length) chunkSize = message.Length - i;
            chunks.Add(message.Substring(i, chunkSize));
        }
        return chunks;
    }
}
