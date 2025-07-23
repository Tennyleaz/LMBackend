using Whisper.net;

namespace LMBackend;

public interface ISttService
{
    public void BuildProcessor();
    public IAsyncEnumerable<SegmentData> WhisperChunk(string audioFile);
}
