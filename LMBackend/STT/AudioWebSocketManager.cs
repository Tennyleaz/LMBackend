using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LMBackend.STT;

public class AudioWebSocketManager : IWebSocketManager
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();

    public void AddSocket(Guid connectionId, WebSocket socket)
    {
        _sockets.TryAdd(connectionId, socket);
    }

    public async Task SendMessageAsync(Guid connectionId, SttResult data)
    {
        if (_sockets.TryGetValue(connectionId, out var socket) && socket.State == WebSocketState.Open)
        {
            string message = JsonSerializer.Serialize(data);
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public Task RemoveSocket(Guid connectionId)
    {
        if (_sockets.TryRemove(connectionId, out var socket))
        {
            return socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by manager", CancellationToken.None);
        }
        return Task.CompletedTask;
    }
}
