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
            if (rawAudioChunk == null)
            {
                // Sleep 1s and wait for next item
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            Console.WriteLine("Createing wav file from: " + rawAudioChunk.File);
            string wavFileName = _audioConverter.ConvertToWav(rawAudioChunk.File); // Implement actual conversion logic
            Console.WriteLine("Createing wav file from: " + rawAudioChunk.File + " done.");
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
