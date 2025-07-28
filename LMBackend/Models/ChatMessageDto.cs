namespace LMBackend.Models;

public class ChatMessageDto
{
    //public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
    public bool UseRetrieval { get; set; }
    public bool UseWebSearch { get; set; }
    public bool UseVoice { get; set; }
}
