using System.Net.WebSockets;
using System.Text;

namespace LMBackend.STT;

internal class WebSocketHandler : IWebSocketHandler
{
    private readonly IAudioQueue _audioQueue;
    private const int SLICE_SEC = 1;
    private const string INPUT_TYPE = "ogg";
    private const int MAX_COMBINE = 5;

    public WebSocketHandler(IAudioQueue audioQueue)
    {
        _audioQueue = audioQueue;
    }

    public async Task HandleAudioWebSocket(WebSocket socket, Guid socketId)
    {
        using MemoryStream bufferStream = new MemoryStream();
        byte[] buffer = new byte[8192];
        string chunkDir = @"C:\temp audio";
        Directory.CreateDirectory(chunkDir);
        bool isStopped = false;
        bool isPaused = false;
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    await bufferStream.WriteAsync(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                // This condition confirms we have received complete blob
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    if (bufferStream.Length > 0)
                    {
                        Guid audioId = Guid.NewGuid();
                        string fileName = $"chunk_{audioId}.{INPUT_TYPE}";
                        fileName = Path.Combine(chunkDir, fileName);
                        await System.IO.File.WriteAllBytesAsync(fileName, bufferStream.ToArray());
                        bufferStream.SetLength(0);  // Clear for next chunk
                        // Add data to audio queue
                        _audioQueue.EnqueueRawData(new RawAudioChunk
                        {
                            Id = audioId,
                            SocketId = socketId,
                            File = fileName,
                            IsLast = isStopped
                        });
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Websocket get close message.");
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Check what command was sent
                    string command = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Command: " + command);
                    if (command == "stop")
                    {
                        isPaused = isStopped = true;
                    }
                    else if (command == "pause")
                    {
                        isPaused = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to write websocket: " + ex);
        }
        finally
        {
            Console.WriteLine("Websocket handler leave...");
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            }
            Console.WriteLine("Websocket handler leave... done.");
        }
    }
}
