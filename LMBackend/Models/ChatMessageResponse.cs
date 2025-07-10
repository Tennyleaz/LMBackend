namespace LMBackend.Models;

/// <summary>
/// Contains the last 2 chat message, and the modified parent chat object (if any).
/// </summary>
public class ChatMessageResponse
{
    public ChatMessageResponse(ChatMessage request, ChatMessage response, ChatDto chatModified)
    {
        // create new object, also prevent circular reference
        Request = new ChatMessage
        {
            Id = request.Id,
            ChatId = request.ChatId,
            Role = request.Role,
            Text = request.Text,
            Timestamp = request.Timestamp,
            Chat = null
        };
        Response = new ChatMessage
        {
            Id = response.Id,
            ChatId = response.ChatId,
            Role = response.Role,
            Text = response.Text,
            Timestamp = response.Timestamp,
            Chat = null
        };
        // this could be null if title does not change
        ChatModified = chatModified;
    }

    public ChatMessage Request { get; set; }
    public ChatMessage Response { get; set; }
    public ChatDto ChatModified { get; set; }
}
