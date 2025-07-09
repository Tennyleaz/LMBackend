namespace LMBackend.Models;

public class ChatMessageResponse
{
    public ChatMessageResponse(ChatMessage request, ChatMessage response)
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
    }

    public ChatMessage Request {  get; set; }
    public ChatMessage Response { get; set; }
}
