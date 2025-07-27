
using System.Net.Http;

namespace LMBackend.STT;

public class TtsService : ITtsService
{
    private readonly string _audioDirectory;
    private readonly HttpClient _httpClient;

    public TtsService()
    {
        _audioDirectory = @"C:\temp audio\TTS";
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(Constants.TTS_ENDPOINT);
        Directory.CreateDirectory(_audioDirectory);
    }

    public void DeleteAudioFile(Guid audioId)
    {
        string fileName = Path.Combine(_audioDirectory, audioId.ToString() + ".wav");
        if (File.Exists(fileName))
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {fileName}: {ex.Message}");
            }
        }
    }

    public FileInfo GetAudioFile(Guid audioId)
    {
        string fileName = Path.Combine(_audioDirectory, audioId.ToString() + ".wav");
        return new FileInfo(fileName);
    }

    public async Task<Guid> TextToSpeech(string text, string locale)
    {
        var body = new
        {
            text = text,
            locale = locale
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("", body);
        if (response.IsSuccessStatusCode)
        {
            Guid audioId = Guid.NewGuid();
            string fileName = Path.Combine(_audioDirectory, audioId.ToString() + ".wav");
            using (FileStream fs = new FileStream(fileName + fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fs);
                return audioId;
            }
        }
        return Guid.Empty;
    }
}
