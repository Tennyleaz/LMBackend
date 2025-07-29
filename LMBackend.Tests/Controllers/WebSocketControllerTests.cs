using LMBackend.Controllers;
using LMBackend.STT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.Controllers.Tests;

[TestFixture()]
public class WebSocketControllerTests
{
    private WebSocketController _controller;
    private Mock<IWebSocketManager> _manager;
    private Mock<IWebSocketHandler> _mockHandler;

    [SetUp]
    public void Setup()
    {
        _manager = new Mock<IWebSocketManager>();
        _mockHandler = new Mock<IWebSocketHandler>();
        _controller = new WebSocketController(_manager.Object, _mockHandler.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test()]
    public async Task Get_TestNotWebsocket()
    {
        // Arrange
        // Manually create the HttpContext with a response stream
        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        // Act
        await _controller.Get();

        // Assert
        Assert.That(context.Response.StatusCode, Is.EqualTo(400));
    }

    [Test()]
    public async Task Get_TestIsWebsocket()
    {
        // Arrange: Mock websocket
        var mockWebSocket = new Mock<WebSocket>();
        var webSocketFeature = new Mock<IHttpWebSocketFeature>();
        webSocketFeature.Setup(ws => ws.AcceptAsync(It.IsAny<WebSocketAcceptContext>())).ReturnsAsync(mockWebSocket.Object);
        webSocketFeature.Setup(ws => ws.IsWebSocketRequest).Returns(true);

        // Mock http context
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpWebSocketFeature>(webSocketFeature.Object);
        context.Request.Path = "/api/ws";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        _mockHandler.Setup(x => x.HandleAudioWebSocket(mockWebSocket.Object, It.IsAny<Guid>())).Verifiable();

        // Act
        await _controller.Get();

        // Assert: handler should handle the websocket
        _mockHandler.Verify(handler => handler.HandleAudioWebSocket(It.Is<WebSocket>(ws => ws == mockWebSocket.Object), It.IsAny<Guid>()), Times.AtLeastOnce);
    }
}