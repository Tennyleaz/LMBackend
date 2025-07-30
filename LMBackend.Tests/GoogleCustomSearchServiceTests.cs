using LMBackend;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.Tests;

[TestFixture()]
public class GoogleCustomSearchServiceTests
{
    [Test()]
    public async Task SearchAsync_Test()
    {
        // Arrange
        Mock<HttpMessageHandler> _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        // Setup the HttpClient to use the mock handler
        HttpClient httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://www.googleapis.com/customsearch/")
        };
        string data = @"{
          ""kind"": ""customsearch#search"",
          ""url"": {
            ""type"": ""application/json"",
            ""template"": ""https://www.googleapis.com/customsearch/v1?q={searchTerms}&...other_parameters...""
          },
          ""queries"": {
            ""request"": [
              {
                ""title"": ""Google Custom Search - example"",
                ""totalResults"": ""1000"",
                ""searchTerms"": ""example"",
                ""count"": 10,
                ""startIndex"": 1,
                ""inputEncoding"": ""utf8"",
                ""outputEncoding"": ""utf8"",
                ""safe"": ""off"",
                ""cx"": ""your_custom_search_engine_id""
              }
            ],
            ""nextPage"": [
              {
                ""title"": ""Google Custom Search - example"",
                ""totalResults"": ""1000"",
                ""searchTerms"": ""example"",
                ""count"": 10,
                ""startIndex"": 11,
                ""inputEncoding"": ""utf8"",
                ""outputEncoding"": ""utf8"",
                ""safe"": ""off"",
                ""cx"": ""your_custom_search_engine_id""
              }
            ]
          },
          ""context"": {
            ""title"": ""Your Custom Search""
          },
          ""items"": [
            {
              ""kind"": ""customsearch#result"",
              ""title"": ""Example Domain"",
              ""htmlTitle"": ""<b>Example</b> Domain"",
              ""link"": ""http://www.example.com/"",
              ""displayLink"": ""www.example.com"",
              ""snippet"": ""This domain is for use in illustrative examples in documents."",
              ""htmlSnippet"": ""This domain is for use in illustrative <b>examples</b> in documents."",
              ""cacheId"": ""1234567890"",
              ""formattedUrl"": ""www.example.com"",
              ""htmlFormattedUrl"": ""www.<b>example</b>.com""
            }
          ]
        }";
        HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
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
        GoogleCustomSearchService service = new GoogleCustomSearchService(httpClient);

        // Act
        List<GoogleSearchResult> results = await service.SearchAsync("coffee", 3);

        // Assert
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results[0].title.Length, Is.GreaterThan(0));
    }

    [Test()]
    public async Task SearchAsync_TestException()
    {
        // Arrange
        Mock<HttpMessageHandler> _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        // Setup the HttpClient to use the mock handler
        HttpClient httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://www.googleapis.com/customsearch/")
        };
        HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage)
            .Verifiable();
        GoogleCustomSearchService service = new GoogleCustomSearchService(httpClient);

        // Act
        List<GoogleSearchResult> results = await service.SearchAsync("coffee", 3);

        // Assert
        Assert.That(results, Is.EqualTo(null));
    }
}