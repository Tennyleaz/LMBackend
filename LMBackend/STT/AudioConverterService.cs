namespace LMBackend.STT;

public class AudioConverterService : BackgroundService
{
    private readonly IAudioQueue _audioQueue;
    private readonly IAudioConverter _audioConverter;

    public AudioConverterService(IAudioQueue audioQueue, IAudioConverter audioConverter)
    {
        _audioQueue = audioQueue;
        _audioConverter = audioConverter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RawAudioChunk rawAudioChunk = _audioQueue.DequeueRawData();
            string wavFileName = _audioConverter.ConverToWav(rawAudioChunk.File); // Implement actual conversion logic
            WavAudioChunk wavAudioChunk = new WavAudioChunk
            {
                Id = rawAudioChunk.Id,
                SocketId = rawAudioChunk.SocketId,
                File = wavFileName,
                IsLast = rawAudioChunk.IsLast
            };
            _audioQueue.EnqueueWavData(wavAudioChunk);
        }
    }
}
