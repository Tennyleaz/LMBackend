using Docker.DotNet.Models;
using LMBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.Controllers.Tests;

[TestFixture()]
public class ModelsControllerTests
{
    private Mock<IConfiguration> _mockConfig;
    private Mock<IDockerHelper> _fakeDocker;
    private ModelsController _controller;

    [SetUp]
    public void Setup()
    {
        // Setup mock config, so JWT will not fail
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(config => config["Jwt:Key"]).Returns("a very long text over 128 bits to prevent IDX10603");
        _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("your-issuer");
        _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("your-audience");

        _fakeDocker = new Mock<IDockerHelper>();
        _controller = new ModelsController(_fakeDocker.Object);
    }

    [Test()]
    public void ListModelsTest()
    {
        // Arrange

        // Act
        var models = _controller.ListModels();

        // Assert
        Assert.That(models.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var okResult = models.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.InstanceOf(typeof(LlmModel[])));
    }

    [Test()]
    public async Task GetCurrentTest()
    {
        // Arrange
        _fakeDocker.Setup(s => s.GetCurrentModel()).ReturnsAsync(new LlmDocker { Model = "test/model" });

        // Act
        var models = await _controller.GetCurrent();

        // Assert
        Assert.That(models.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var okResult = models.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.InstanceOf(typeof(LlmDocker)));
    }

    [Test()]
    public async Task SetCurrent_TestOk()
    {
        // Arrange
        string modelName = "test/model";
        _fakeDocker.Setup(s => s.ChangeCurrentModel(modelName)).ReturnsAsync(new LlmDocker { Model = modelName });
        LlmDockerDto dto = new LlmDockerDto { Model = modelName };

        // Act
        var setResult = await _controller.SetCurrent(dto);

        // Assert
        Assert.That(setResult.Result, Is.InstanceOf(typeof(OkObjectResult)));
        var okResult = setResult.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.InstanceOf(typeof(LlmDocker)));
        var newDocker = okResult.Value as LlmDocker;
        Assert.That(newDocker?.Model, Is.EqualTo(modelName));
    }

    [Test()]
    public async Task SetCurrent_TestFail()
    {
        // Arrange
        string modelName = "test/model";
        _fakeDocker.Setup(s => s.ChangeCurrentModel(modelName)).ReturnsAsync((LlmDocker)null);
        LlmDockerDto dto = new LlmDockerDto { Model = modelName };

        // Act
        var setResult = await _controller.SetCurrent(dto);

        // Assert
        Assert.That(setResult.Result, Is.InstanceOf(typeof(ObjectResult)));
        var statusCodeResult = setResult.Result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult?.StatusCode, Is.EqualTo(500));
    }

    [Test()]
    public async Task HealthCheck_TestOk()
    {
        // Arrange
        string modelName = "test/model";
        _fakeDocker.Setup(s => s.GetCurrentContainer()).ReturnsAsync(new ContainerListResponse { State = "running" });
        _fakeDocker.Setup(s => s.GetCurrentModelName()).ReturnsAsync(modelName);
        _fakeDocker.Setup(s => s.CheckMetrics(modelName)).ReturnsAsync(true);

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task HealthCheck_Returns500()
    {
        // Arrange
        _fakeDocker.Setup(s => s.GetCurrentContainer()).ReturnsAsync(new ContainerListResponse { State = "stopped" });

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>()); // 500 response will be an ObjectResult, not OkResult
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult?.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task HealthCheck_Returns504()
    {
        // Arrange
        _fakeDocker.Setup(s => s.GetCurrentContainer()).ReturnsAsync(new ContainerListResponse { State = "running" });
        _fakeDocker.Setup(s => s.GetCurrentModelName()).ReturnsAsync("test/model");
        _fakeDocker.Setup(s => s.CheckMetrics(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult?.StatusCode, Is.EqualTo(504));
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
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