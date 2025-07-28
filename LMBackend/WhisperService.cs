using Whisper.net;
using Whisper.net.Logger;

namespace LMBackend;

public class WhisperService : ISttService, IDisposable
{    
    private readonly WhisperFactory whisperFactory;
    private WhisperProcessor processor;

    public WhisperService()
    {
        using var whisperLogger = LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);
        whisperFactory = WhisperFactory.FromPath("C:\\whisper models\\ggml-base.bin");
    }

    public void BuildProcessor()
    {
        if (processor == null)
        {
            processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();
        }
        Console.WriteLine("Whisper factory build.");
    }

    public async IAsyncEnumerable<SegmentData> WhisperChunk(string audioFile)
    {
        using var fileStream = System.IO.File.OpenRead(audioFile);
        await foreach (SegmentData result in processor.ProcessAsync(fileStream))
        {
            Console.WriteLine($"{result.Start}->{result.End} ({result.Language}): {result.Text}");
            yield return result;
        }
    }


    public void Dispose()
    {
        whisperFactory?.Dispose();
        processor?.Dispose();
    }
}
