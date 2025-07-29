using LMBackend.Controllers;
using LMBackend.Models;
using LMBackend.STT;
using LMBackend.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.Controllers.Tests;

[TestFixture()]
public class SpeechControllerTests
{
    private Mock<ITtsService> _ttsService;
    private SpeechController _controller;

    [SetUp]
    public void Setup()
    {
        _ttsService = new Mock<ITtsService>();
        _controller = new SpeechController(_ttsService.Object);
    }

    [Test()]
    public async Task SpeechToText_TestOk()
    {
        // Arrange
        string text = "hello";
        string fileName = @"C:\temp audio\test.txt";
        if (!File.Exists(fileName))
            File.Create(fileName);
        Guid id = Guid.NewGuid();
        FileInfo fakeFile = new FileInfo(fileName);

        // Mocking the service methods
        _ttsService.Setup(x => x.TextToSpeech(text, SpeechLocale.English)).ReturnsAsync(id);
        _ttsService.Setup(x => x.GetAudioFile(id)).Returns(fakeFile);
        _ttsService.Setup(x => x.DeleteAudioFile(id)).Verifiable();
        SpeechRequest request = new SpeechRequest { Locale = SpeechLocale.English, Text = text };

        // Manually create the HttpContext with a response stream
        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        // Act
        IActionResult result = await _controller.SpeechToText(request);

        // Assert
        Assert.That(result, Is.InstanceOf<FileStreamResult>());
        FileStreamResult fileResult = result as FileStreamResult;
        Assert.That(fileResult.ContentType, Is.EqualTo("audio/wav"));
        // Ensure the delete method is called
        await context.Response.CompleteAsync();
    }

    [Test()]
    public async Task SpeechToText_TestNotFound()
    {
        // Arrange
        string text = "hello";
        string fileName = @"C:\temp audio\test.txt";
        if (!File.Exists(fileName))
            File.Create(fileName);
        Guid id = Guid.NewGuid();
        FileInfo fakeFile = new FileInfo(fileName);

        // Mocking the service methods
        _ttsService.Setup(x => x.TextToSpeech(text, SpeechLocale.English)).ReturnsAsync(id);
        _ttsService.Setup(x => x.GetAudioFile(id)).Returns(null as FileInfo);
        SpeechRequest request = new SpeechRequest { Locale = SpeechLocale.English, Text = text };

        // Act
        IActionResult result = await _controller.SpeechToText(request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }
}