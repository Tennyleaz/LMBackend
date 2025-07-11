using Docker.DotNet;
using Docker.DotNet.Models;
using LMBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace LMBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModelsController : Controller
{
    private readonly ChatContext _context;
    private readonly DockerClient _dockerClient;
    private static readonly LlmModel[] _models = new LlmModel[]
    {
        new LlmModel("meta-llama/Llama-3.2-3B-Instruct", ""),
        new LlmModel("Qwen/Qwen3-4B", ""),
        new LlmModel("google/gemma-3n-E4B-it", ""),
    };

    public ModelsController(ChatContext context)
    {
        _context = context;
        _dockerClient = new DockerClientConfiguration(new Uri(Constants.DOCKER_ENDPOINT)).CreateClient();
    }

    /// <summary>
    /// Get all available models.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    //[Authorize]
    public ActionResult<LlmModel[]> ListModels()
    {
        return Ok(_models);
    }

    /// <summary>
    /// Get current vLLM docker status.
    /// </summary>
    /// <returns></returns>
    [HttpGet("docker")]
    //[Authorize]
    public async Task<ActionResult<LlmDocker>> GetCurrent()
    {
        CancellationToken ct = HttpContext.RequestAborted;
        IList<ContainerListResponse> containers = await _dockerClient.Containers.ListContainersAsync(
            new ContainersListParameters() {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "name", 
                        new Dictionary<string, bool>
                        {
                            { Constants.DOCKER_NAME, true}
                        }
                    }
                }
            },
            ct);
        if (containers.Count == 0)
        {
            return NotFound(Constants.DOCKER_NAME);
        }

        ContainerListResponse clr = containers.First();
        // Find model in the command, like:
        // python3 -m vllm.entrypoints.openai.api_server --model meta-llama/Llama-3.2-3B-Instruct --api-key tenny --max-model-len 8192
        const string token = "--model ";
        int start = clr.Command.IndexOf(token) + token.Length;
        int end = clr.Command.IndexOf(" ", start + 1);
        string model = clr.Command.Substring(start, end - start);
        // Check if the model is recognized
        foreach (var m in _models)
        {
            if (m.Name == model)
            {
                LlmDocker response = new LlmDocker
                {
                    Status = clr.Status,  // "Up 3 hours"
                    State = clr.State,  // "running"
                    Model = model  // "meta-llama/Llama-3.2-3B-Instruct"
                };
                return Ok(response);
            }
        }

        return NotFound(model);
    }

    /// <summary>
    /// Set current vLLM docker status.
    /// </summary>
    /// <returns></returns>
    [HttpPost("docker")]
    [Authorize]
    public async Task<ActionResult<LlmDocker>> SetCurrent(LlmDockerDto request)
    {
        // Stop the docker

        // Create command based on model

        // Start the docker

        // Tell client container has started
        await Task.Delay(100);
        return Ok();
    }
}
