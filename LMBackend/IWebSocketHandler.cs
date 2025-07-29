using System.Net.WebSockets;

namespace LMBackend;

public interface IWebSocketHandler
{
    Task HandleAudioWebSocket(WebSocket webSocket, Guid connectionId);
}
