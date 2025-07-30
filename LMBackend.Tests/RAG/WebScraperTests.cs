using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.RAG.Tests;

[TestFixture()]
internal class WebScraperTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private WebScraper _webScraper;

    [SetUp]
    public void Setup()
    {
        // Create the mock HttpMessageHandler with Moq
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Setup the HttpClient to use the mock handler
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };

        _webScraper = new WebScraper(httpClient);
    }

    [Test]
    public async Task Scrap_TestOk()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ \"title\": \"news\", \"text\": \"hello\", \"error\": \"\" }") // Mocked JSON response
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage)
            .Verifiable();

        // Act
        string result = await _webScraper.Scrap("https://example.com");

        // Assert
        Assert.That(result, Is.TypeOf<string>());
        Assert.That(result, Is.EqualTo("hello"));

        _httpMessageHandlerMock.Verify(); // Verifies SendAsync was called.
    }

    [Test]
    public async Task Scrap_TestException()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {

        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception("Mock exception"));

        // Act
        string result = await _webScraper.Scrap("https://example.com");

        // Assert
        Assert.That(result, Is.EqualTo(null));
    }
}
