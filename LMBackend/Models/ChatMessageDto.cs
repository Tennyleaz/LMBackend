namespace LMBackend.Models;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
