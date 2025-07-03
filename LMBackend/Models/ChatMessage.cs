namespace LMBackend.Models;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Role Role { get; set; }
    public string Text { get; set; }
    public uint Timestamp { get; set; }
}
