using System.Net.WebSockets;

namespace LMBackend.STT;

public interface IWebSocketManager
{
    void AddSocket(Guid connectionId, WebSocket socket);
    Task SendMessageAsync(Guid connectionId, SttResult data);
    Task RemoveSocket(Guid connectionId);
}
