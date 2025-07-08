namespace LMBackend.Models;

public class ChatMessageStreamResponse
{
    public Guid ChatId { get; set; }
    public Guid MessageId { get; set; }
    // Starts at 0
    public int Sequence { get; set; }
    public StreamStatus Status { get; set; }
    public uint Timestamp { get; set; }
    // Not empty only if Status is InProgress.
    public string Text { get; set; }
}
