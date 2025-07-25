using LMBackend.RAG;

namespace LMBackend;

public interface ISerpService
{
    public Task<SerpResultSchema> SearchGoogle(string query);

    public Task<string> SearchGoogleWithString(string query);
}
