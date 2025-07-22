using Docker.DotNet.Models;
using LMBackend.Models;

namespace LMBackend;

public interface IDockerHelper
{
    public Task<string> GetCurrentModelName();
    public Task<LlmDocker> GetCurrentModel();
    public Task<LlmDocker> ChangeCurrentModel(string modelName);
    public Task<ContainerListResponse> GetCurrentContainer();
    public Task<bool> CheckMetrics(string modelName);
}
