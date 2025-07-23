using LMBackend;
using LMBackend.Controllers;
using LMBackend.Models;
using LMBackend.Tests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using OpenAI.VectorStores;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.Controllers.Tests;

[TestFixture()]
public class DocumentsControllerTests
{
    private Mock<IConfiguration> _mockConfig;
    private Mock<ILlmService> _llmService;
    private Mock<IVectorStoreService> _vectorService;
    private ChatContext _context;
    private DocumentsController _controller;

    [SetUp]
    public async Task Setup()
    {
        // Setup chat context and database
        var options = new DbContextOptionsBuilder<ChatContext>().UseInMemoryDatabase(databaseName: "TestDatabase").Options;
        _context = new ChatContext(options);

        // Seed data into the InMemory database
        Guid userId = Guid.NewGuid();
        _context.Users.Add(new User { Id = userId, Name = "Test User" });
        _context.Documents.Add(new Document { ChatId = Guid.Empty, UserId = userId, Id = Guid.NewGuid(), CreatedTime = DateTime.UtcNow, Name = "Test doc" });
        _context.SaveChanges();

        // Setup mock config, so JWT will not fail
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(config => config["Jwt:Key"]).Returns("a very long text over 128 bits to prevent IDX10603");
        _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("your-issuer");
        _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("your-audience");

        _llmService = new Mock<ILlmService>();
        _vectorService = new Mock<IVectorStoreService>();

        _controller = new DocumentsController(_context, _llmService.Object, _vectorService.Object);
    }

    [Test()]
    public async Task GetDocuments_TestUnauthorized()
    {
        // Arrange

        // Act
        ActionResult<IEnumerable<Document>> result = await _controller.GetDocuments();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<IEnumerable<Document>>>());
        UnauthorizedResult objectResult = result.Result as UnauthorizedResult;
        Assert.That(objectResult, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test()]
    public async Task GetDocuments_TestOk()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);

        // Act
        ActionResult<IEnumerable<Document>> result = await _controller.GetDocuments();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<IEnumerable<Document>>>());
        IEnumerable<Document> docs = result.Value;
        Assert.That(docs, Is.InstanceOf<IEnumerable<Document>>());
    }

    [Test()]
    public async Task GetDocument_TestNotFound()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid docId = Guid.NewGuid();

        // Act
        ActionResult<Document> result = await _controller.GetDocument(docId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Document>>());
        NotFoundResult notfoundResult = result.Result as NotFoundResult;  // the result is the actual NotFound()
        Assert.That(notfoundResult, Is.InstanceOf<NotFoundResult>());  // ensure 404
    }

    [Test()]
    public async Task GetDocument_TestOk()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid docId = _context.Documents.First().Id;

        // Act
        ActionResult<Document> result = await _controller.GetDocument(docId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Document>>());
        OkObjectResult objecResult = result.Result as OkObjectResult;
        Assert.That(objecResult, Is.InstanceOf<OkObjectResult>());
        Document doc = objecResult.Value as Document;
        Assert.That(doc.Id, Is.EqualTo(docId));
    }

    [Test()]
    public async Task PostDocument_TestUnauthorized()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        byte[] buffer = Encoding.UTF8.GetBytes("Hello world!");
        DocumentDto dto = new DocumentDto
        {
            Name = "Doc 2.txt",
            CreatedTime = DateTime.UtcNow,
            Data = buffer,
        };

        // Act
        ActionResult<Document> result = await _controller.PostDocument(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Document>>());
        UnauthorizedResult objecResult = result.Result as UnauthorizedResult;
        Assert.That(objecResult, Is.InstanceOf<UnauthorizedResult>());  // ensure 404
    }

    [Test()]
    public async Task PostDocument_TestNotFound()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        JwtMock.PrepareMockJwt(_controller, userId);
        byte[] buffer = Encoding.UTF8.GetBytes("Hello world!");
        DocumentDto dto = new DocumentDto
        {
            Name = "Doc 2.txt",
            CreatedTime = DateTime.UtcNow,
            Data = buffer,
        };

        // Act
        ActionResult<Document> result = await _controller.PostDocument(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Document>>());
        NotFoundObjectResult notfoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notfoundResult, Is.InstanceOf<NotFoundObjectResult>());  // ensure 404
    }

    [Test()]
    public async Task PostDocument_Test500()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        byte[] buffer = Encoding.UTF8.GetBytes("Hello world!");
        DocumentDto dto = new DocumentDto
        {
            Name = "Doc 2.txt",
            CreatedTime = DateTime.UtcNow,
            Data = buffer,
        };

        // Act
        ActionResult<Document> result = await _controller.PostDocument(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Document>>());
        ObjectResult objecResult = result.Result as ObjectResult;
        Assert.That(objecResult, Is.InstanceOf<ObjectResult>());
        Assert.That(objecResult.StatusCode, Is.EqualTo(500));
    }

    [Test()]
    public async Task PostDocument_TestCreted()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        byte[] buffer = Encoding.UTF8.GetBytes("Hello world!");
        DocumentDto dto = new DocumentDto
        {
            Name = "Doc 2.txt",
            CreatedTime = DateTime.UtcNow,
            Data = buffer,
        };

        _vectorService.Setup(x => x.UpsertAsync(It.IsAny<string>(), new List<RAG.ChromaChunk>())).ReturnsAsync(true);

        // Act
        ActionResult<Document> result = await _controller.PostDocument(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Document>>());
        CreatedAtActionResult objecResult = result.Result as CreatedAtActionResult;
        Assert.That(objecResult, Is.InstanceOf<CreatedAtActionResult>());
        Document doc = objecResult.Value as Document;
        Assert.That(doc, Is.InstanceOf<Document>());
        Assert.That(doc.Name, Is.EqualTo(dto.Name));
    }

    [Test()]
    public async Task DeleteDocument_TestNotFound()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid docId = Guid.NewGuid();

        // Act
        IActionResult actionResult = await _controller.DeleteDocument(docId);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
    }

    [Test()]
    public async Task DeleteDocument_TestUnauthorized()
    {
        // Arrange
        Guid docId = _context.Documents.First().Id;

        // Act
        IActionResult actionResult = await _controller.DeleteDocument(docId);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test()]
    public async Task DeleteDocument_TestNoContent()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid docId = _context.Documents.First().Id;

        // Act
        IActionResult actionResult = await _controller.DeleteDocument(docId);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<NoContentResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _controller.Dispose();
    }
}