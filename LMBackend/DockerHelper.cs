using Docker.DotNet;
using Docker.DotNet.Models;
using LMBackend.Models;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LMBackend;

internal class DockerHelper : IDockerHelper
{    
    private readonly DockerClient _dockerClient;
    private string _currentModel;

    public DockerHelper()
    {
        _dockerClient = new DockerClientConfiguration(new Uri(Constants.DOCKER_ENDPOINT)).CreateClient();
    }
    
    private async Task TryStrartDefaultDocker()
    {
        ContainerListResponse container = await GetCurrentContainer();
        if (container == null)
        {
            // Create command based on model name
            CreateContainerParameters createParams = CreateDockerParams(Constants.DEFAULT_MODEL);

            // Create and start the docker container
            CreateContainerResponse createResponse = await _dockerClient.Containers.CreateContainerAsync(createParams);
            if (await _dockerClient.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters { }))
            {
                _currentModel = Constants.DEFAULT_MODEL;
            }
        }
    }

    public async Task<string> GetCurrentModelName()
    {
        if (!string.IsNullOrEmpty(_currentModel))
        {
            return _currentModel;
        }

        LlmDocker dockerModel = await GetCurrentModel();
        if (dockerModel == null)
        {
            return Constants.DEFAULT_MODEL;
        }
        return dockerModel.Model;
    }

    public async Task<LlmDocker> GetCurrentModel()
    {
        // Create clients first
        await TryStrartDefaultDocker();

        // Get current running one
        ContainerListResponse clr = await GetCurrentContainer();
        LlmDocker dockerModel = MatchCommandModelName(clr);

        // Remember it
        _currentModel = dockerModel.Model;
        return dockerModel;
    }

    public async Task<LlmDocker> ChangeCurrentModel(string modelName)
    {
        // Find and stop the docker container
        ContainerListResponse container = await GetCurrentContainer();
        if (container != null)
        {
            string containerId = container.ID;
            await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });

            // Remove the docker container
            await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters { });
        }

        // Create command based on model name
        CreateContainerParameters createParams = CreateDockerParams(modelName);

        // Create and start the docker container
        CreateContainerResponse createResponse = await _dockerClient.Containers.CreateContainerAsync(createParams);
        if (await _dockerClient.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters { }))
        {
            // Remember it
            _currentModel = modelName;
            // Get again and return
            return await GetCurrentModel();
        }
        return null;
    }

    public async Task<ContainerListResponse> GetCurrentContainer()
    {
        IList<ContainerListResponse> containers = await _dockerClient.Containers.ListContainersAsync(CreateListParams());
        return containers.FirstOrDefault();
    }

    private static ContainersListParameters CreateListParams()
    {
        return new ContainersListParameters()
        {
            All = true,  // We need stopped containers too!
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
        };
    }

    private static CreateContainerParameters CreateDockerParams(string modelName)
    {
        string token = Environment.GetEnvironmentVariable("HUGGINGFACE_TOKEN");
        var param = new CreateContainerParameters
        {
            Image = "vllm/vllm-openai:latest",
            Name = Constants.DOCKER_NAME,
            Env = new List<string>
            {
                $"HUGGING_FACE_HUB_TOKEN={token}"
            },
            HostConfig = new HostConfig
            {
                Runtime = "nvidia",
                IpcMode = "host",
                DeviceRequests = new List<DeviceRequest>
                {
                    new DeviceRequest
                    {
                        Driver = "nvidia",
                        DeviceIDs = new List<string> { "3" },
                        Capabilities = new List<IList<string>>
                        {
                            new List<string> { "gpu" }
                        }
                    }
                },
                Binds = new List<string>
                {
                    "/home/phison/.cache/huggingface:/root/.cache/huggingface",
                    "/home/phison/tenny_lu/chat_templates:/app/templates"
                },
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { "8000/tcp", new List<PortBinding> { new PortBinding { HostPort = "9090" } } }
                },
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { "8000/tcp", default }
            },
            // vLLM container's commands here
            Cmd = new List<string>
            {
                "--model", modelName,
                "--api-key", Constants.LLM_KEY, 
                "--gpu_memory_utilization", "0.8"
            }
        };
        // Set custom parameters
        if (modelName == "meta-llama/Llama-3.2-3B-Instruct" || modelName == "unsloth/Llama-3.2-3B-Instruct")
        {
            param.Cmd.Add("--max-model-len");
            param.Cmd.Add("5000");
            param.Cmd.Add("--enable-auto-tool-choice");
            param.Cmd.Add("--tool-call-parser");
            param.Cmd.Add("llama3_json");
            param.Cmd.Add("--chat-template");
            param.Cmd.Add("/app/templates/llama3.2.jinja");
        }
        else if (modelName == "Qwen/Qwen3-4B")
        {
            param.Cmd.Add("--enable-auto-tool-choice");
            param.Cmd.Add("--tool-call-parser");
            param.Cmd.Add("hermes");
            param.Cmd.Add("--chat-template");
            param.Cmd.Add("/app/templates/qwen3.2.jinja");
        }
        else if (modelName == "google/gemma-3-4b-it" || modelName == "google/gemma-3n-E2B-it")
        {
            // -enable-auto-tool-choice     --tool-call-parser pythonic     --chat-template tool_chat_template_gemma3_json.jinja
            param.Cmd.Add("--enable-auto-tool-choice");
            param.Cmd.Add("--tool-call-parser");
            param.Cmd.Add("pythonic");
            param.Cmd.Add("--chat-template");
            param.Cmd.Add("tool_chat_template_gemma3_json.jinja");
        }
        return param;
    }

    private static LlmDocker MatchCommandModelName(ContainerListResponse clr)
    {
        // Find model in the command, like:
        // python3 -m vllm.entrypoints.openai.api_server --model meta-llama/Llama-3.2-3B-Instruct --api-key tenny --max-model-len 8192
        const string token = "--model ";
        int start = clr.Command.IndexOf(token) + token.Length;
        int end = clr.Command.IndexOf(" ", start + 1);
        string model = clr.Command.Substring(start, end - start);
        // Check if the model is recognized
        foreach (var m in ALL)
        {
            if (m.Name == model)
            {
                LlmDocker response = new LlmDocker
                {
                    Status = clr.Status,  // "Up 3 hours"
                    State = clr.State,  // "running"
                    Model = model  // "meta-llama/Llama-3.2-3B-Instruct"
                };
                return response;
            }
        }
        // Not found match model
        return null;
    }

    /// <summary>
    /// Check "/metrics" endpoint available for 2s.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckMetrics(string modelName)
    {
        HttpClient http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(2); // Short timeout per attempt

        //const int checkCount = 25;
        bool ready = false;
        //for (int i = 0; i < checkCount; i++)
        {
            try
            {
                var requestMsg = new HttpRequestMessage(HttpMethod.Post, Constants.METRICS_ENDPOINT);
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Constants.LLM_KEY);
                var response = await http.SendAsync(requestMsg);

                if (response.IsSuccessStatusCode)
                {
                    // check for "vllm:num_requests_running{engine="0",model_name="meta-llama/Llama-3.2-3B-Instruct"} 0.0"
                    // which may be 0.0
                    string responseString = await response.Content.ReadAsStringAsync();
                    string[] lines = responseString.Split("\n");
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("vllm:num_requests_running{"))
                        {
                            // Find model_name="
                            if (line.Contains(modelName))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                // Service might not be up yet, ignore and retry
            }
            catch (Exception)
            {

            }
            // Wait 1s before next try
            await Task.Delay(1000);
        }

        return ready;
    }

    public static readonly LlmModel[] ALL = new LlmModel[]
    {
        new LlmModel("meta-llama/Llama-3.2-3B-Instruct", ""),
        new LlmModel("unsloth/Llama-3.2-3B-Instruct", ""),
        new LlmModel("Qwen/Qwen3-4B", ""),
        new LlmModel("google/gemma-3-4b-it", ""),
        new LlmModel("google/gemma-3n-E2B-it", ""),
    };
}
