using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LMBackend;

internal class LlmClient
{
    private readonly ChatClient _client;
    private readonly HttpClient _httpClient;

    public static LlmClient Instance { get; private set; }

    public static async Task TryCreateLlmInstance()
    {
        if (Instance != null)
            return;
        string modelName = await DockerHelper.GetCurrentModelName();
        Instance = new LlmClient(Constants.LLM_KEY, modelName);
    }

    public string Model { get; private set; }

    private LlmClient(string apiKey, string model)
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
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(Constants.EMBEDDING_ENDPOINT);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Constants.LLM_KEY);
    }

    public async Task<string> GetChatResult(string question)
    {
        UserChatMessage userMessage = new UserChatMessage(question);
        SystemChatMessage systemMessage = new SystemChatMessage("You are a helpful assistant.");
        ChatMessage[] messages = { systemMessage, userMessage };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages);
        return RemoveThink(result.Value.Content[0].Text);
    }

    public async IAsyncEnumerable<string> GetChatResultStreaming(List<Models.ChatMessage> oldMessages, string question, string ragResult)
    {
        // Determine system prompt based on RAG
        string systemPrompt;
        if (string.IsNullOrEmpty(ragResult))
        {
            systemPrompt = "You are a helpful assistant.";
        }
        else
        {
            systemPrompt = "Use the context below to answer the question as best as you can. If the answer is not in the context or not relvent, notify the user.\n\nContext:\n\n";
            systemPrompt += ragResult;
        }
        List<ChatMessage> messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt) // add once, at start
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

    public async Task<float[]> GetEmbedding(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var payload = new 
        {
            model = "Qwen/Qwen3-Embedding-0.6B",
            input = text,
            user = "tenny"
        };
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/v1/embeddings", payload);
            // "{"object":"error","message":"The model does not support Embeddings API","type":"BadRequestError","param":null,"code":400}"
            // "{"id":"embd-e46f57901d0b48a8951e1099df8375c7","object":"list","created":1752660182,"model":"Qwen/Qwen3-Embedding-0.6B","data":[{"index":0,"object":"embedding","embedding":[...]}]}
            EmbeddingResult result = await response.Content.ReadFromJsonAsync<EmbeddingResult>();
            if (result.code == 200 || result.code == 0)
            {
                return result.data[0].embedding;
            }
            Console.WriteLine("Error embed text: " + result.message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error embed text: " + ex);
            return null;
        }
    }
}

internal class EmbeddingResult
{
    public string id { get; set; }
    public uint created { get; set; }
    public string model { get; set; }
    public int code { get; set; }
    public string message { get; set; }
    public EmbeddingData[] data { get; set; }
}

internal class EmbeddingData
{
    public int index { get; set; }
    public float[] embedding { get; set; }
}