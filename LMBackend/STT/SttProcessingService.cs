using Whisper.net;

namespace LMBackend.STT;

public class SttProcessingService : BackgroundService
{
    private readonly IAudioQueue _audioQueue;
    private readonly ISttService _sttService;
    private readonly IWebSocketManager _webSocketManager;

    public SttProcessingService(
        IAudioQueue audioQueue,
        ISttService sttService,
        IWebSocketManager webSocketManager)
    {
        _audioQueue = audioQueue;
        _sttService = sttService;
        _webSocketManager = webSocketManager;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            WavAudioChunk chunk = _audioQueue.DequeueWavData();

            // Process audio with STT
            IAsyncEnumerable<SegmentData> datas = _sttService.WhisperChunk(chunk.File);
            await foreach (SegmentData data in datas)
            {
                // Send transcription result back to the client
                double start = data.Start.TotalSeconds;
                double end = data.End.TotalSeconds;
                SttResult sttResult = new SttResult
                {
                    isStopped = chunk.IsLast,
                    start = start,
                    end = end,
                    text = data.Text
                };
                await _webSocketManager.SendMessageAsync(chunk.SocketId, sttResult);
            }
        }
    }
}
