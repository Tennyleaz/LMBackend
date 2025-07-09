namespace LMBackend.Models;

public class Chat
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedTime { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();

    // foreign key of User id
    public Guid UserId { get; set; }
    // navigation property of User object
    public virtual User User { get; set; }

    public static Chat FromDto(ChatDto dto)
    {
        return new Chat
        {
            Id = dto.Id,
            Title = dto.Title,
            CreatedTime = dto.CreatedTime,
        };
    }
}
