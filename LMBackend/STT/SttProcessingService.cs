using Whisper.net;

namespace LMBackend.STT;

public class SttProcessingService : BackgroundService
{
    private readonly IAudioQueue _audioQueue;
    private readonly ISttService _sttService;
    private readonly IWebSocketManager _webSocketManager;
    private readonly ILlmService _llmService;

    public SttProcessingService(
        IAudioQueue audioQueue,
        ISttService sttService,
        IWebSocketManager webSocketManager,
        ILlmService llmService)
    {
        _audioQueue = audioQueue;
        _sttService = sttService;
        _webSocketManager = webSocketManager;
        _llmService = llmService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            WavAudioChunk chunk = _audioQueue.DequeueWavData();
            if (chunk == null)
            {
                // Sleep 1s and wait for next item
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            // Process audio with STT
            Console.WriteLine("Calling whisper... " + chunk.File);
            IAsyncEnumerable<SegmentData> datas = _sttService.WhisperChunk(chunk.File);
            await foreach (SegmentData data in datas)
            {
                // Ask LLM to fix possible errors
                string text = await _llmService.CorrectSpeechResult(data.Text);

                // Send transcription result back to the client
                double start = data.Start.TotalSeconds;
                double end = data.End.TotalSeconds;
                SttResult sttResult = new SttResult
                {
                    isStopped = chunk.IsLast,
                    start = start,
                    end = end,
                    text = text,
                    language = data.Language
                };
                await _webSocketManager.SendMessageAsync(chunk.SocketId, sttResult);
            }

            // Delete completed files
            try
            {
                File.Delete(chunk.File);
            }
            catch { }
        }
    }
}
