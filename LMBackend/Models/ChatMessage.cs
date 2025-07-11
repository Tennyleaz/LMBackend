namespace LMBackend.Models;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Role Role { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
    // only has value for Role=System
    public string Model { get; set; }

    // foreign key of Chat id
    public Guid ChatId { get; set; }
    // navigation property of Chat object
    public Chat Chat { get; set; }

    public static ChatMessage FromDto(ChatMessageDto dto)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            Role = Role.User,
            Text = dto.Text,
            Timestamp = dto.Timestamp
        };
    }
}
