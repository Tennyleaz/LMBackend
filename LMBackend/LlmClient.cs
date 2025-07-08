using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace LMBackend;

internal class LlmClient
{
    private readonly ChatClient _client;

    public LlmClient(string apiKey, string model)
    {
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
        ChatMessage[] messages = { userMessage, systemMessage };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages);
        return result.Value.Content[0].Text;
    }
}
