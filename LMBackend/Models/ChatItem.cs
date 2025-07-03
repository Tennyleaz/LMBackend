namespace LMBackend.Models;

public class ChatItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<ChatMessage> Messages { get; set; }    
}
