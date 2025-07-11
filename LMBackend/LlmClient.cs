using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace LMBackend;

internal class LlmClient
{
    private readonly ChatClient _client;

    public string Model { get; private set; }

    public LlmClient(string apiKey, string model)
    {
        Model = model;
        _client = new ChatClient(
            options: new OpenAIClientOptions()
            {
                Endpoint = new Uri(Constants.LLM_ENDPOINT)
            },
            model: model,
            credential: new ApiKeyCredential(apiKey)
        );
    }

    public async Task<string> GetChatResult(string question)
    {
        UserChatMessage userMessage = new UserChatMessage(question);
        SystemChatMessage systemMessage = new SystemChatMessage("You are a helpful assistant.");
        ChatMessage[] messages = { systemMessage, userMessage };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages);
        return RemoveThink(result.Value.Content[0].Text);
    }

    public async IAsyncEnumerable<string> GetChatResultStreaming(List<Models.ChatMessage> oldMessages, string question)
    {
        List<ChatMessage> messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant.") // add once, at start
        };
        // Add history from this chat
        List<Models.ChatMessage> limitedHistory = ChatHistoryLimiter.LimitHistory(oldMessages, Constants.MAX_TOKEN, question);
        foreach (var history in limitedHistory)
        {
            if (history.Role == Models.Role.System)
            {
                messages.Add(new AssistantChatMessage(history.Text));
            }
            else
            {
                messages.Add(new UserChatMessage(history.Text));
            }
        }
        // Add the new question
        messages.Add(new UserChatMessage(question));
        // Call LLM API
        AsyncCollectionResult<StreamingChatCompletionUpdate> updates = _client.CompleteChatStreamingAsync(messages);
        await foreach (StreamingChatCompletionUpdate completionUpdate in updates)
        {
            foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
            {
                yield return contentPart.Text;
            }
        }
    }

    public async Task<string> GetChatTitle(string question)
    {
        UserChatMessage userMessage = new UserChatMessage(question);
        SystemChatMessage systemMessage = new SystemChatMessage("Generate a short, 1-line summary for user's question, in few words, in plain text.");
        ChatMessage[] messages = { userMessage, systemMessage };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages);
        return RemoveThink(result.Value.Content[0].Text);
    }

    private static string RemoveThink(string text)
    {
        //int start = text.IndexOf("<think>");
        int end = text.IndexOf("</think>") + 8;
        if (end > 0)
        {
            return text.Substring(end, text.Length - end);
        }
        return text;
    }
}
