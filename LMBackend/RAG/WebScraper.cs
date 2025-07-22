using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace LMBackend.RAG;

internal class WebScraper
{
    private readonly HttpClient _client;

    public WebScraper()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(Constants.SCRAP_ENDPOINT);
    }


    public async Task<string> Scrap(string url)
    {
        var payload = new
        {
            url = url
        };
        try
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("", payload);
            ScrapResult json = await response.Content.ReadFromJsonAsync<ScrapResult>();
            return json.text;
            // decode the text
            //string decodedString = Regex.Unescape(json.text);
            //return decodedString;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to scrape url: " + ex);
            return null;
        }
    }
}

internal class ScrapResult
{
    public string title { get; set; }
    public string text { get; set; }
    public string error { get; set; }
}
