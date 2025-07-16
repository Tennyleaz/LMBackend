namespace LMBackend.Models;

public class Document
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedTime { get; set; }

    // foreign key of Chat id
    public Guid ChatId { get; set; }
    // foreign key of User id
    public Guid UserId { get; set; }
    // navigation property of User object
    public virtual User User { get; set; }

    public static Document FromDto(DocumentDto dto, Guid userId)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CreatedTime = dto.CreatedTime,
            ChatId = dto.ChatId,
            UserId = userId
        };
    }
}
