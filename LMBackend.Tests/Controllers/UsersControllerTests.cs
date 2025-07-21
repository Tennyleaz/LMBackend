using LMBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMBackend.Controllers.Tests;

[TestFixture()]
public class UsersControllerTests
{
    private Mock<IConfiguration> _mockConfig;
    private ChatContext _context;
    private UsersController _controller;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ChatContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

        _context = new ChatContext(options);

        // Seed data into the InMemory database
        _context.Users.Add(new User { Id = Guid.NewGuid(), Name = "Test User" });
        _context.SaveChanges();

        // Setup mock config, so JWT will not fail
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(config => config["Jwt:Key"]).Returns("a very long text over 128 bits to prevent IDX10603");
        _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("your-issuer");
        _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("your-audience");

        _controller = new UsersController(_context, _mockConfig.Object);
    }

    [Test()]
    public async Task RegisterTest()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest 
        {
            UserName = "random new user",
            Password = "123456"
        };

        // Act
        IActionResult result = await _controller.Register(request);

        // Assert
        Assert.That(result, Is.InstanceOf(typeof(OkResult)));
    }

    [Test()]
    public async Task Register_TestDuplicateName()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            UserName = "Test User",
            Password = "123456"
        };

        // Act
        IActionResult result = await _controller.Register(request);

        // Assert
        Assert.That(result, Is.InstanceOf(typeof(BadRequestObjectResult)));
    }

    [Test()]
    public async Task LoginTest()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "login test new user",
            HashedPassword = BCrypt.Net.BCrypt.HashPassword("123")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest 
        { 
            UserName = "login test new user",
            Password = "123"
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        // Ok() with payload is a OkObjectResult !!
        // Ok() without payload is a OkResult !!
        Assert.That(result, Is.InstanceOf(typeof(OkObjectResult)));
    }

    [Test()]
    public async Task Login_TestWrongPassword()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "tenny",
            HashedPassword = BCrypt.Net.BCrypt.HashPassword("123")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            UserName = "tenny",
            Password = "456"
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf(typeof(UnauthorizedResult)));
    }

    [Test()]
    public async Task GetUserTest()
    {
        // Arrange
        var userId = _context.Users.First().Id; // Get the existing user's ID

        // Act
        ActionResult<UserDto> result = await _controller.GetUser(userId);

        // Assert
        Assert.That(result, Is.InstanceOf(typeof(ActionResult<UserDto>)));
        Assert.That(userId, Is.EqualTo(result.Value?.Id));
    }

    [Test]
    public async Task GetMe_ShouldReturnUnauthorized_WhenUserIdIsEmpty()
    {
        // Arrange
        var controllerContext = new ControllerContext();
        var httpContext = new DefaultHttpContext();
        controllerContext.HttpContext = httpContext;

        // Mock the User object to return an empty GUID
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("sub", string.Empty)
        }));

        _controller.ControllerContext = controllerContext;

        // Act
        var result = await _controller.GetMe();

        // Assert
        Assert.That(result.Result, Is.InstanceOf(typeof(UnauthorizedResult)));
    }

    [Test]
    public async Task GetMe_ShouldReturnOk_WithUserData_WhenUserExists()
    {
        // Arrange
        var user = _context.Users.First(); // Get the existing user's ID
        var userId = user.Id;
        PrepareMockJwt(userId);

        // Act
        var result = await _controller.GetMe();

        // Assert
        Assert.That(result.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedUser = okResult.Value as User;
        Assert.That(user, Is.EqualTo(returnedUser));
    }


    [Test()]
    public async Task DeleteUser_TestRandomId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        Assert.That(result, Is.InstanceOf(typeof(UnauthorizedResult)));
    }

    [Test()]
    public async Task DeleteUser_TestActualId()
    {
        // Arrange
        var userId = _context.Users.First().Id; // Get the existing user's ID
        PrepareMockJwt(userId);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        Assert.That(result, Is.InstanceOf(typeof(NoContentResult)));  // returns 204 no content
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private void PrepareMockJwt(Guid userId)
    {
        // Prepare middleware
        var controllerContext = new ControllerContext();
        var httpContext = new DefaultHttpContext();
        controllerContext.HttpContext = httpContext;

        // Mock the User object to return a valid userId in JWT subject
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("sub", userId.ToString())
        }));

        _controller.ControllerContext = controllerContext;
    }
}