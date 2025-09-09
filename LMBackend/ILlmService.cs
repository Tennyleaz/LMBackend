namespace LMBackend;

public interface ILlmService
{
    public Task<string> GetModelName();
    public Task<string> GetChatResult(string question);
    public IAsyncEnumerable<string> GetChatResultStreaming(List<Models.ChatMessage> oldMessages, string question, string ragResult, bool useWebSearch, bool useVoice, CancellationToken ct);
    public Task<string> GetChatTitle(string question);
    public Task<GoogleSearchKeyword> GetGoogleSearchKeyword(string question);
    public Task<string> SummarizeWebpage(string html, string query);
    public Task<string> CorrectSpeechResult(string text);
    public Task<float[]> GetEmbedding(string text);
    public Task<IEnumerable<float[]>> GetEmbeddingBatch(string[] text);
}
