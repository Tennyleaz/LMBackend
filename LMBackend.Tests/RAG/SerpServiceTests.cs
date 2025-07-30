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
internal class SerpServiceTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private SerpService _serpService;

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

        _serpService = new SerpService(httpClient);
    }

    [Test]
    public async Task SearchGoogle_TestOk()
    {
        // Arrange
        string data = @"{
          ""organic_results"": [
            {
              ""position"": 1,
              ""title"": ""Coffee"",
              ""link"": ""https://en.wikipedia.org/wiki/Coffee"",
              ""snippet"": ""Coffee is a beverage prepared from roasted coffee beans."",
              ""snippet_highlighted_words"": [
                ""Coffee"",
                ""coffee""
              ]
	        }]
        }";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(data) // Mocked JSON response
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
        SerpResultSchema result = await _serpService.SearchGoogle("coffee");

        // Assert
        Assert.That(result, Is.TypeOf<SerpResultSchema>());
        Assert.That(result.organic_results.Count, Is.EqualTo(1));

        _httpMessageHandlerMock.Verify(); // Verifies SendAsync was called.
    }

    [Test]
    public async Task SearchGoogle_TestException()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception("Mock exception"));

        // Act
        SerpResultSchema result = await _serpService.SearchGoogle("coffee");

        // Assert
        Assert.That(result, Is.EqualTo(null));
    }

    [Test]
    public async Task SearchGoogleWithString_TestOk()
    {
        // Arrange
        string data = @"{
          ""organic_results"": [
            {
              ""position"": 1,
              ""title"": ""Coffee"",
              ""link"": ""https://en.wikipedia.org/wiki/Coffee"",
              ""snippet"": ""Coffee is a beverage prepared from roasted coffee beans."",
              ""snippet_highlighted_words"": [
                ""Coffee"",
                ""coffee""
              ]
	        }]
        }";
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(data) // Mocked JSON response
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
        string result = await _serpService.SearchGoogleWithString("coffee");

        // Assert
        Assert.That(result, Is.TypeOf<string>());
        Assert.That(result.Length, Is.GreaterThan(0));

        _httpMessageHandlerMock.Verify(); // Verifies SendAsync was called.
    }
}
