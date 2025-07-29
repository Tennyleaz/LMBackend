using Asp.Versioning;
using LMBackend.STT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Whisper.net;

namespace LMBackend.Controllers;

[Route("/api/v{version:apiVersion}/ws")]
[ApiController]
[ApiVersion("1.0")]
public class WebSocketController : Controller
{
    private readonly ISttService _sttService;
    private readonly IWebSocketManager _webSocketManager;
    private readonly IAudioQueue _audioQueue;
    private readonly ConcurrentQueue<SttChunk> rawQueue = new ConcurrentQueue<SttChunk>();
    private readonly ConcurrentQueue<SttChunk> wavFileQueue = new ConcurrentQueue<SttChunk>();
    private const int SLICE_SEC = 1;
    private const string INPUT_TYPE = "ogg";
    private const int MAX_COMBINE = 5;

    public WebSocketController(IWebSocketManager webSocketManager, IAudioQueue audioQueue)
    {
        _webSocketManager = webSocketManager;
        _audioQueue = audioQueue;
    }

    [HttpGet]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            Guid connectionId = Guid.NewGuid();
            _webSocketManager.AddSocket(connectionId, webSocket);
            try
            {
                await HandleAudioWebSocket(webSocket, connectionId);
            }
            finally
            {
                await _webSocketManager.RemoveSocket(connectionId);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task HandleAudioWebSocket(WebSocket socket, Guid socketId)
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

    private struct SttChunk
    {
        public string File;
        public int Start;
        public int End;
        public bool IsLast;
    }
}
