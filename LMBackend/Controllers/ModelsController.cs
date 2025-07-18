using Docker.DotNet;
using Docker.DotNet.Models;
using LMBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Asp.Versioning;

namespace LMBackend.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class ModelsController : Controller
{
    private readonly ChatContext _context;

    public ModelsController(ChatContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all available models.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    //[Authorize]
    public ActionResult<LlmModel[]> ListModels()
    {
        return Ok(DockerHelper.ALL);
    }

    /// <summary>
    /// Get current vLLM docker status.
    /// </summary>
    /// <returns></returns>
    [HttpGet("docker")]
    //[Authorize]
    public async Task<ActionResult<LlmDocker>> GetCurrent()
    {
        LlmDocker dockerModel = await DockerHelper.GetCurrentModel();
        if (dockerModel != null)
        {
            return Ok(dockerModel);
        }
        return NotFound(Constants.DOCKER_NAME);
    }

    /// <summary>
    /// Set current vLLM docker.
    /// </summary>
    /// <returns></returns>
    [HttpPost("docker")]
    //[Authorize]
    public async Task<ActionResult<LlmDocker>> SetCurrent(LlmDockerDto request)
    {
        LlmDocker dockerModel = await DockerHelper.ChangeCurrentModel(request.Model);
        if (dockerModel != null)
        {
            return Ok(dockerModel);
        }
        return StatusCode(500, "vLLM container failed to start!");
    }

    /// <summary>
    /// Check the vLLM docker status is ready or not.
    /// </summary>
    /// <returns></returns>
    [HttpGet("healthCheck")]
    public async Task<IActionResult> HealthCheck()
    {
        // Check if it is running first       
        ContainerListResponse container = await DockerHelper.GetCurrentContainer();
        if (container.State != "created" && container.State != "running")
        {
            return StatusCode(500, "vLLM container state error: " + container.State);
        }

        // Check if model name is not empty
        string modelName = await DockerHelper.GetCurrentModelName();

        // Check metrics endpoint status once
        bool isReady = await DockerHelper.CheckMetrics(modelName);
        if (isReady) 
        {
            return Ok();           
        }
        return StatusCode(504, "vLLM /metrics endpoint not ready in time.");
    }
}
