namespace LMBackend.Models;

public class ChatMessageStreamResponse
{
    public Guid ChatId { get; set; }
    public Guid MessageId { get; set; }
    public Guid ReplyMessageId { get; set; }
    // Starts at 0
    public int Sequence { get; set; }
    public StreamStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    // Not empty only if Status is InProgress.
    public string Text { get; set; }
    public string Model { get; set; }
    public ChatDto ChatModified { get; set; }
    public string Error { get; set; }
    // True if request is using speech
    public bool UseVoice { get; set; }
}
