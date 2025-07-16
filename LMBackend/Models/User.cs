namespace LMBackend.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string HashedPassword { get; set; }
    public virtual List<Chat> Chats { get; set; } = new();
    public virtual List<Document> Documents { get; set; } = new();
}
