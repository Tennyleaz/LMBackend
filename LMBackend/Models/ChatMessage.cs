namespace LMBackend.Models;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Role Role { get; set; }
    public string Text { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    // foreign key of Chat id
    public Guid ChatId { get; set; }
    // navigation property of Chat object
    public Chat Chat { get; set; }
}
