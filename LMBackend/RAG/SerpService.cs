using System.Net;
using System.Text.Json;

namespace LMBackend.RAG;

internal class SerpService : ISerpService
{
    private readonly HttpClient _httpClient;

    public SerpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SerpResultSchema> SearchGoogle(string query)
    {
        string apiKey = Environment.GetEnvironmentVariable("SERP_API_KEY");
        string param = $"?q={WebUtility.UrlEncode(query)}&api_key={apiKey}";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(param);
            string contentJson = await response.Content.ReadAsStringAsync();

            // Find each search result from serp
            SerpResultSchema result = JsonSerializer.Deserialize<SerpResultSchema>(contentJson);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to search using SERP: " + ex);
            return null;
        }
    }

    public async Task<string> SearchGoogleWithString(string query)
    {
        SerpResultSchema searchResult = await SearchGoogle(query);
        string toolResult = "";
        if (searchResult != null && searchResult.organic_results.Length > 0)
        {
            // Summarize the json to text
            foreach (SerpOrganicResult o in searchResult.organic_results)
            {
                toolResult += "\n" + JsonSerializer.Serialize(o);
            }
        }
        return toolResult;
    }
}

public class SerpResultSchema
{
    //public string search_metadata { get; set; }
    //public string search_parameters { get; set; }
    //public string search_information { get; set; }
    public SerpOrganicResult[] organic_results { get; set; }
}

public class SerpOrganicResult
{
    public int position { get; set; }
    public string title { get; set; }
    public string link { get; set; }
    public string snippet { get; set; }
    public string[] snippet_highlighted_words { get; set; }
}
