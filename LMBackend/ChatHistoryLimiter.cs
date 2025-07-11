using LMBackend.Models;

namespace LMBackend;

internal static class ChatHistoryLimiter
{
    public static List<ChatMessage> LimitHistory(List<ChatMessage> oldMessages, int tokenLimit, string question)
    {
        // Calculate user propmt size        
        int tokenTotal = EstimateTokens(question);
        List<ChatMessage> includedMessages = new List<ChatMessage>();

        // Newest history first
        IEnumerable<ChatMessage> reversedHistory = oldMessages.AsEnumerable().Reverse();
        foreach (ChatMessage msg in reversedHistory)
        {
            int msgTokens = EstimateTokens(msg.Text);
            if (tokenTotal + msgTokens > tokenLimit)
                break;
            includedMessages.Add(msg);
            tokenTotal += msgTokens;
        }

        // Now reverse to keep chronological order
        includedMessages.Reverse();
        return includedMessages;
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Length / 3;
    }
}
