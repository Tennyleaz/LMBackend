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
    private readonly IWebSocketManager _webSocketManager;
    private readonly IWebSocketHandler _webSocketHandler;

    public WebSocketController(IWebSocketManager webSocketManager, IWebSocketHandler webSocketHandler)
    {
        _webSocketManager = webSocketManager;
        _webSocketHandler = webSocketHandler;
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
                await _webSocketHandler.HandleAudioWebSocket(webSocket, connectionId);
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

    
    private struct SttChunk
    {
        public string File;
        public int Start;
        public int End;
        public bool IsLast;
    }
}
