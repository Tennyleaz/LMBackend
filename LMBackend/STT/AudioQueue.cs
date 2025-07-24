using System.Collections.Concurrent;

namespace LMBackend.STT;

public class AudioQueue : IAudioQueue
{
    private readonly ConcurrentQueue<RawAudioChunk> _rawDataQueue = new();
    private readonly ConcurrentQueue<WavAudioChunk> _wavQueue = new();

    public void EnqueueRawData(RawAudioChunk data)
    {
        _rawDataQueue.Enqueue(data);
    }

    public RawAudioChunk DequeueRawData()
    {
        _rawDataQueue.TryDequeue(out var item);
        return item;
    }

    public void EnqueueWavData(WavAudioChunk data)
    {
        _wavQueue.Enqueue(data);
    }

    public WavAudioChunk DequeueWavData()
    {
        _wavQueue.TryDequeue(out var item);
        return item;
    }
}