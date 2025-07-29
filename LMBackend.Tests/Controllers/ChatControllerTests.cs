using LMBackend.Models;
using LMBackend.RAG;
using LMBackend.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using OpenAI.VectorStores;
using System.Security.Claims;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace LMBackend.Controllers.Tests;

[TestFixture()]
public class ChatControllerTests
{
    private Mock<IConfiguration> _mockConfig;
    private Mock<ILlmService> _llmService;
    private Mock<ISerpService> _serpService;
    private Mock<IVectorStoreService> _vectorService;
    private ChatContext _context;
    private ChatController _controller;

    [SetUp]
    public void Setup()
    {
        // Setup InMemory database for testing
        var options = new DbContextOptionsBuilder<ChatContext>().UseInMemoryDatabase(databaseName: "TestDatabase").Options;
        _context = new ChatContext(options);

        // Seed data into the InMemory database
        Guid userId = Guid.NewGuid();
        _context.Users.Add(new User { Id = userId, Name = "Test User" });
        _context.SaveChanges();
        Chat chat = new Chat
        {
            Id = Guid.NewGuid(),
            UserId = userId, // Assign the first user's ID
            Title = "Test Chat",
            CreatedTime = DateTime.UtcNow,
        };
        _context.Chats.Add(chat);
        ChatMessage message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatId = chat.Id,
            Text = "Test chat message.",
            Model = "test/model",
            Role = Role.User,
            Timestamp = DateTime.UtcNow,
        };
        _context.ChatMessages.Add(message);
        _context.SaveChanges();

        // Setup mock config, so JWT will not fail
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(config => config["Jwt:Key"]).Returns("a very long text over 128 bits to prevent IDX10603");
        _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("your-issuer");
        _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("your-audience");

        _llmService = new Mock<ILlmService>();
        _serpService = new Mock<ISerpService>();
        _vectorService = new Mock<IVectorStoreService>();

        _controller = new ChatController(_context, _llmService.Object, _serpService.Object, _vectorService.Object);
    }

    [Test()]
    public async Task GetChats_TestNotFound()
    {
        // Arrange
        // Do not set JWT, so it will return Unauthorized

        // Act
        ActionResult<IEnumerable<Chat>> result = await _controller.GetChats();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<IEnumerable<Chat>>>());
        UnauthorizedResult unauthorized = result.Result as UnauthorizedResult;  // the result is the actual NotFound()
        Assert.That(unauthorized, Is.InstanceOf<UnauthorizedResult>());  // ensure 404
    }

    [Test()]
    public async Task GetChats_TestOk()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);

        // Act
        ActionResult<IEnumerable<Chat>> result = await _controller.GetChats();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<IEnumerable<Chat>>>());
        IEnumerable<Chat> chats = result.Value;
        Assert.That(chats, Is.InstanceOf<IEnumerable<Chat>>());
    }

    [Test()]
    public async Task GetChat_TestNotFound()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);

        // Act
        ActionResult<Chat> result = await _controller.GetChat(Guid.NewGuid());  // random id that does not exist

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Chat>>());
        NotFoundResult notfoundResult = result.Result as NotFoundResult;  // the result is the actual NotFound()
        Assert.That(notfoundResult, Is.InstanceOf<NotFoundResult>());  // ensure 404
    }

    [Test()]
    public async Task GetChat_TestOk()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID

        // Act
        ActionResult<Chat> result = await _controller.GetChat(chatId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Chat>>());
        OkObjectResult objecResult = result.Result as OkObjectResult;
        Assert.That(objecResult, Is.InstanceOf<OkObjectResult>());
        Chat chat = objecResult.Value as Chat;
        Assert.That(chat.Id, Is.EqualTo(chatId));
    }

    [Test()]
    public async Task GetChatMessages_TestNotFound()
    {
        // Arrange

        // Act
        ActionResult<IEnumerable<ChatMessage>> actionResult = await _controller.GetChatMessages(Guid.NewGuid());  // random id that does not exist

        // Assert
        Assert.That(actionResult, Is.InstanceOf<ActionResult<IEnumerable<ChatMessage>>>());
        NotFoundResult notfoundResult = actionResult.Result as NotFoundResult;  // the result is the actual NotFound()
        Assert.That(notfoundResult, Is.InstanceOf<NotFoundResult>());  // ensure 404
    }

    [Test()]
    public async Task GetChatMessages_TestOk()
    {
        // Arrange
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID

        // Act
        ActionResult<IEnumerable<ChatMessage>> actionResult = await _controller.GetChatMessages(chatId);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<ActionResult<IEnumerable<ChatMessage>>>());       
        IEnumerable<ChatMessage> messages = actionResult.Value;
        Assert.That(messages, Is.InstanceOf<IEnumerable<ChatMessage>>());
    }

    [Test()]
    public async Task PutChat_TestBadRequest()
    {
        // Arrange
        Guid chatId = _context.Chats.First().Id;
        ChatDto dto = new ChatDto
        {
            Id = chatId,  // This is the existing chat's ID
            Title = "Updated Chat Title",
            CreatedTime = DateTime.UtcNow
        };

        // Act
        IActionResult actionResult = await _controller.PutChat(Guid.NewGuid(), dto);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<BadRequestResult>());
    }

    [Test()]
    public async Task PutChat_TestNotFound()
    {
        // Arrange
        Guid chatId = Guid.NewGuid();
        ChatDto dto = new ChatDto
        {
            Id = chatId,  // This is the existing chat's ID
            Title = "Updated Chat Title",
            CreatedTime = DateTime.UtcNow
        };

        // Act
        IActionResult actionResult = await _controller.PutChat(chatId, dto);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test()]
    public async Task PutChat_TestOk()
    {
        // Arrange
        Guid chatId = _context.Chats.First().Id;
        ChatDto dto = new ChatDto
        {
            Id = chatId,  // This is the existing chat's ID
            Title = "Updated Chat Title",
            CreatedTime = DateTime.UtcNow
        };

        // Act
        IActionResult actionResult = await _controller.PutChat(chatId, dto);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<NoContentResult>());
    }

    [Test()]
    public async Task PostChat_TestUnauthorized()
    {
        // Arrange
        Guid chatId = Guid.NewGuid();
        ChatDto dto = new ChatDto
        {
            Id = chatId,  // This is the existing chat's ID
            Title = "New Chat Title",
            CreatedTime = DateTime.UtcNow
        };

        // Act
        ActionResult<Chat> result = await _controller.PostChat(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Chat>>());
        UnauthorizedResult objecResult = result.Result as UnauthorizedResult;
        Assert.That(objecResult, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test()]
    public async Task PostChat_TestNotFound()
    {
        // Arrange
        Guid userId = Guid.NewGuid(); // Create a new user ID that does not exist
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = Guid.NewGuid();
        ChatDto dto = new ChatDto
        {
            Id = chatId,  // This is the existing chat's ID
            Title = "New Chat Title",
            CreatedTime = DateTime.UtcNow
        };

        // Act
        ActionResult<Chat> result = await _controller.PostChat(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Chat>>());
        NotFoundObjectResult objecResult = result.Result as NotFoundObjectResult;
        Assert.That(objecResult, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test()]
    public async Task PostChat_TestConflict()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID
        ChatDto dto = new ChatDto
        {
            Id = chatId,  // This is the existing chat's ID
            Title = "New Chat Title",
            CreatedTime = DateTime.UtcNow
        };

        // Act
        ActionResult<Chat> result = await _controller.PostChat(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Chat>>());
        ConflictObjectResult objecResult = result.Result as ConflictObjectResult;
        Assert.That(objecResult, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test()]
    public async Task PostChat_TestCreated()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = Guid.NewGuid(); // Create a new chat ID
        ChatDto dto = new ChatDto
        {
            Id = chatId,  // This is the existing chat's ID
            Title = "New Chat Title",
            CreatedTime = DateTime.UtcNow
        };

        // Act
        ActionResult<Chat> result = await _controller.PostChat(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<Chat>>());
        CreatedAtActionResult objecResult = result.Result as CreatedAtActionResult;
        Assert.That(objecResult, Is.InstanceOf<CreatedAtActionResult>());
        Chat chat = objecResult.Value as Chat;
        Assert.That(chat.Id, Is.EqualTo(chatId));
    }

    [Test()]
    public async Task PostChatMessage_TestNotFound()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = Guid.NewGuid();
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow
        };

        // Act
        ActionResult<ChatMessageResponse> result = await _controller.PostChatMessage(chatId, dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<ChatMessageResponse>>());
        NotFoundObjectResult objecResult = result.Result as NotFoundObjectResult;
        Assert.That(objecResult, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test()]
    public async Task PostChatMessage_TestOk()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        // Create new chat with no messages
        Guid chatId = Guid.NewGuid();
        Chat chat = new Chat
        {
            Id = chatId,
            UserId = userId, // Assign the first user's ID
            Title = "Test Chat",
            CreatedTime = DateTime.UtcNow,
        };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow
        };
        // Mock create title
        _llmService.Setup(x => x.GetChatTitle(It.IsAny<string>())).ReturnsAsync("Fake chat title");

        // Act
        ActionResult<ChatMessageResponse> result = await _controller.PostChatMessage(chatId, dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<ChatMessageResponse>>());
        OkObjectResult objecResult = result.Result as OkObjectResult;
        Assert.That(objecResult, Is.InstanceOf<OkObjectResult>());
        ChatMessageResponse chatMessage = objecResult.Value as ChatMessageResponse;
        Assert.That(chatMessage.Request.Text, Is.EqualTo(dto.Text));
        Assert.That(chatMessage.Response.ChatId, Is.EqualTo(chatId));
        Assert.That(chatMessage.Response.Role, Is.EqualTo(Role.System));
    }

    [Test()]
    public async Task PostChatMessage_TestLlmAggregateException()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow
        };
        // Mock throw exception
        _llmService.Setup(x => x.GetChatResult(It.IsAny<string>())).Throws(new AggregateException("Mock exception"));

        // Act
        ActionResult<ChatMessageResponse> result = await _controller.PostChatMessage(chatId, dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<ChatMessageResponse>>());
        BadRequestObjectResult objecResult = result.Result as BadRequestObjectResult;
        Assert.That(objecResult, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test()]
    public async Task PostChatMessage_TestLlmException()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow
        };
        // Mock throw exception
        _llmService.Setup(x => x.GetChatResult(It.IsAny<string>())).Throws(new Exception("Mock exception"));

        // Act
        ActionResult<ChatMessageResponse> result = await _controller.PostChatMessage(chatId, dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<ChatMessageResponse>>());
        BadRequestObjectResult objecResult = result.Result as BadRequestObjectResult;
        Assert.That(objecResult, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test()]
    public async Task StreamMessageAsJson_TestOk()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        // Create new chat with no messages
        Guid chatId = Guid.NewGuid();
        Chat chat = new Chat
        {
            Id = chatId,
            UserId = userId, // Assign the first user's ID
            Title = "Test Chat",
            CreatedTime = DateTime.UtcNow,
        };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        // Mock create title
        _llmService.Setup(x => x.GetChatTitle(It.IsAny<string>())).ReturnsAsync("Fake chat title");
        // Mock return embedding array
        float[] fakeEmbedding = new float[1024];
        _llmService.Setup(x => x.GetEmbedding(It.IsAny<string>())).ReturnsAsync(fakeEmbedding);
        // Mock RAG search
        _vectorService.Setup(x => x.TryCreateCollection(userId)).ReturnsAsync("123");
        Dictionary<string, object> metadata = new Dictionary<string, object>
        {
            { "documentName", "aaa" },
            { "chunkIndex", 1 }
        };
        QueryResponse fakeQ = new QueryResponse
        {
            Distances = new List<ICollection<float?>> { new List<float?> { 0, 1 } },  // this is null by default!
            Ids = { new List<string> { "aa", "bb" } },  // this NOT null
            Documents = new List<ICollection<string>> { new List<string> { "AA", "BB" } },  // this is null by default!
            Metadatas = new List<ICollection<Dictionary<string, object>>> { new List<Dictionary<string, object>> { metadata, metadata } },  // this is null by default!
            Embeddings = new List<ICollection<ICollection<float>>> {  },
            Uris = null,
            AdditionalProperties = null
        };
        IList<ChromaRagChunkResult> fakeResult = ChromaRagChunkResult.FromQueryResponse(fakeQ);
        _vectorService.Setup(x => x.SearchAsync(It.IsAny<string>(), chatId, fakeEmbedding, 5, null)).ReturnsAsync(fakeResult);
        // Mock success result
        IAsyncEnumerable<string> streamingTexts = CreateFakeChatText();
        _llmService.Setup(x => x.GetChatResultStreaming(chat.Messages, It.IsAny<string>(), It.IsAny<string>(), false, false, new CancellationToken())).Returns(streamingTexts);
        // Create DTO
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow,
            UseRetrieval = true
        };

        // Act
        await _controller.StreamMessageAsJson(chatId, dto);

        // Assert
        Assert.Pass("Stream success.");
    }

    [Test()]
    public async Task StreamMessageAsJson_TestNoUserId()
    {
        // Arrange
        Guid userId = Guid.Empty;
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _controller.StreamMessageAsJson(chatId, dto);

        // Assert
        Assert.Pass("Stream success, but will return error.");
    }

    [Test()]
    public async Task StreamMessageAsJson_TestNoChatId()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = Guid.NewGuid();
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _controller.StreamMessageAsJson(chatId, dto);

        // Assert
        Assert.Pass("Stream success, but will return error.");
    }

    [Test()]
    public async Task StreamMessageAsJson_TestChatTitleException()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        // Create new chat with no messages
        Guid chatId = Guid.NewGuid();
        Chat chat = new Chat
        {
            Id = chatId,
            UserId = userId, // Assign the first user's ID
            Title = "Test Chat",
            CreatedTime = DateTime.UtcNow,
        };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        // Mock throw exception
        _llmService.Setup(x => x.GetChatTitle(It.IsAny<string>())).Throws(new Exception("Mock exception"));
        // Create DTO
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow
        };        

        // Act
        await _controller.StreamMessageAsJson(chatId, dto);

        // Assert
        Assert.Pass("Stream success, but will return error.");
    }

    [Test()]
    public async Task StreamMessageAsJson_TestRagEmbeddingException()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow,
            UseRetrieval = true
        };
        // Mock return null
        _llmService.Setup(x => x.GetEmbedding(It.IsAny<string>())).ReturnsAsync(null as float[]);

        // Act
        await _controller.StreamMessageAsJson(chatId, dto);

        // Assert
        Assert.Pass("Stream success, but will return error.");
    }

    [Test()]
    public async Task StreamMessageAsJson_TestRagSearchException()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow,
            UseRetrieval = true
        };
        // Mock return array
        float[] fakeEmbedding = new float[1024];
        _llmService.Setup(x => x.GetEmbedding(It.IsAny<string>())).ReturnsAsync(fakeEmbedding);
        // Mock return null
        _vectorService.Setup(x => x.SearchAsync(It.IsAny<string>(), chatId, fakeEmbedding, null, null)).ReturnsAsync(null as IList<ChromaRagChunkResult>);

        // Act
        await _controller.StreamMessageAsJson(chatId, dto);

        // Assert
        Assert.Pass("Stream success, but will return error.");
    }

    [Test()]
    public async Task StreamMessageAsJson_TestLlmException()
    {
        // Arrange
        Guid userId = _context.Users.First().Id; // Get the existing user's ID
        JwtMock.PrepareMockJwt(_controller, userId);
        Chat chat = _context.Chats.First();
        Guid chatId = _context.Chats.First().Id; // Get the existing chat's ID
        ChatMessageDto dto = new ChatMessageDto
        {
            Text = "New chat message",
            Timestamp = DateTime.UtcNow,
        };
        // Mock throw exception
        _llmService.Setup(x => x.GetChatResultStreaming(chat.Messages, It.IsAny<string>(), null, false, false, new CancellationToken())).Throws(new Exception("Mock exception"));

        // Act
        await _controller.StreamMessageAsJson(chatId, dto);

        // Assert
        Assert.Pass("Stream success, but will return error.");
    }

    [Test()]
    public async Task DeleteChat_TestNotFound()
    {
        // Arrange
        Guid chatId = Guid.NewGuid();

        // Act
        IActionResult actionResult = await _controller.DeleteChat(chatId);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<NotFoundResult>());
    }

    [Test()]
    public async Task DeleteChat_TestNoContent()
    {
        // Arrange
        Guid chatId = _context.Chats.First().Id;

        // Act
        IActionResult actionResult = await _controller.DeleteChat(chatId);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<NoContentResult>());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private async IAsyncEnumerable<string> CreateFakeChatText()
    {
        string text = "Hello.";
        foreach (char c in text)
        {
            yield return c.ToString();
        }
    }
}