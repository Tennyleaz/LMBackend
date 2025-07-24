namespace LMBackend.STT;

public interface IAudioQueue
{
    void EnqueueRawData(RawAudioChunk data);
    RawAudioChunk DequeueRawData();

    void EnqueueWavData(WavAudioChunk data);
    WavAudioChunk DequeueWavData();
}
