namespace LMBackend.Models;

public class ChatDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
