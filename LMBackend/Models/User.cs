namespace LMBackend.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string HashedPassword { get; set; }
    public virtual List<Chat> Chats { get; set; } = new();
}
