﻿using LMBackend.RAG;
using OpenAI;
using OpenAI.Chat;
using System.Buffers;
using System.ClientModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LMBackend;

internal class LlmClient : ILlmService
{
    private ChatClient _client;
    private readonly HttpClient _httpClient;
    private readonly IDockerHelper _dockerHelper;
    private readonly ISerpService _serpService;
    private ChatTool serpSearchTool;

    public LlmClient(IDockerHelper dockerHelper, ISerpService serpService)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(Constants.EMBEDDING_ENDPOINT);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Constants.LLM_KEY);
        _dockerHelper = dockerHelper;
        _serpService = serpService;
    }

    private async Task TryCreateChatClient()
    {
        string model = await _dockerHelper.GetCurrentModelName();
        if (_client == null)
        {
            _client = new ChatClient(
                options: new OpenAIClientOptions()
                {
                    Endpoint = new Uri(Constants.LLM_ENDPOINT)
                },
                model: model,
                credential: new ApiKeyCredential(Constants.LLM_KEY)
            );
        }

        serpSearchTool = ChatTool.CreateFunctionTool("serp_search_tool", "Searc Google using serp API.",
            BinaryData.FromString(
            @"
            {
              ""type"": ""object"",
              ""properties"": {
                ""keywords"": {
                  ""type"": ""string"",
                  ""description"": ""The keywords to search Google.""
                }
              },
              ""required"": [""keywords""]
            }
            "), true);
    }

    public async Task<string> GetModelName()
    {
        return await _dockerHelper.GetCurrentModelName();
    }

    public async Task<string> GetChatResult(string question)
    {
        await TryCreateChatClient();

        UserChatMessage userMessage = new UserChatMessage(question);
        SystemChatMessage systemMessage = new SystemChatMessage("You are a helpful assistant.");
        List<ChatMessage> messages = new List<ChatMessage> { systemMessage, userMessage };
        ChatCompletionOptions options = new ChatCompletionOptions
        {
            AllowParallelToolCalls = false,
            ToolChoice = ChatToolChoice.CreateAutoChoice(),
            Tools = { serpSearchTool }
        };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages, options);
        if (result.Value.ToolCalls?.Count > 0)
        {
            if (result.Value.ToolCalls[0].FunctionName == "serp_search_tool")
            {
                string jsonParam = result.Value.ToolCalls[0].FunctionArguments?.ToString() ?? "";
                SerpParam searchParam = JsonSerializer.Deserialize<SerpParam>(jsonParam);
                SerpResultSchema searchResult = await _serpService.SearchGoogle(searchParam.keywords);
                string toolResult = "";
                if (searchResult != null && searchResult.organic_results.Length > 0)
                {
                    // Summarize the json to text
                    foreach (SerpOrganicResult o in searchResult.organic_results)
                    {
                        toolResult += "\n" + JsonSerializer.Serialize(o);
                    }
                    // Add new tool result back to messages
                    messages.Add(new ToolChatMessage(result.Value.ToolCalls[0].Id, toolResult));
                    result = await _client.CompleteChatAsync(messages, options);
                    return RemoveThink(result.Value.Content[0].Text);
                }
            }
        }
        return RemoveThink(result.Value.Content[0].Text);
    }

    public async IAsyncEnumerable<string> GetChatResultStreaming(List<Models.ChatMessage> oldMessages, string question, string ragResult)
    {
        await TryCreateChatClient();

        // Determine system prompt based on RAG
        string systemPrompt;
        if (string.IsNullOrEmpty(ragResult))
        {
            systemPrompt = "You are a helpful assistant.";
        }
        else
        {
            systemPrompt = "Use the context below to answer the question as best as you can. If the answer is not in the context or not relvent, notify the user.\n\nContext:\n\n";
            systemPrompt += ragResult;
        }
        List<ChatMessage> messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt) // add once, at start
        };
        // Add history from this chat
        List<Models.ChatMessage> limitedHistory = ChatHistoryLimiter.LimitHistory(oldMessages, Constants.MAX_TOKEN, question);
        foreach (var history in limitedHistory)
        {
            if (history.Role == Models.Role.System)
            {
                messages.Add(new AssistantChatMessage(history.Text));
            }
            else
            {
                messages.Add(new UserChatMessage(history.Text));
            }
        }
        // Add the new question
        messages.Add(new UserChatMessage(question));
        // Add tool call
        ChatCompletionOptions options = new ChatCompletionOptions
        {
            AllowParallelToolCalls = false,
            ToolChoice = ChatToolChoice.CreateAutoChoice(),
            Tools = { serpSearchTool }
        };
        // Call LLM API
        bool requiresAction;
        do
        {
            requiresAction = false;            
            StringBuilder contentBuilder = new();
            StreamingChatToolCallsBuilder toolCallsBuilder = new();
            
            AsyncCollectionResult<StreamingChatCompletionUpdate> updates = _client.CompleteChatStreamingAsync(messages, options);
            await foreach (StreamingChatCompletionUpdate completionUpdate in updates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    //contentBuilder.Append(contentPart.Text);
                    yield return contentPart.Text;
                }

                // Build the tool calls as new updates arrive.
                foreach (StreamingChatToolCallUpdate toolCallUpdate in completionUpdate.ToolCallUpdates)
                {
                    toolCallsBuilder.Append(toolCallUpdate);
                }

                // See:
                // https://github.com/openai/openai-dotnet/blob/main/examples/Chat/Example04_FunctionCallingStreamingAsync.cs
                switch (completionUpdate.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        //yield return contentBuilder.ToString();
                        break;
                    case ChatFinishReason.ToolCalls:
                        // First, collect the accumulated function arguments into complete tool calls to be processed
                        IReadOnlyList<ChatToolCall> toolCalls = toolCallsBuilder.Build();

                        // Next, add the assistant message with tool calls to the conversation history.
                        AssistantChatMessage assistantMessage = new(toolCalls);
                        if (contentBuilder.Length > 0)
                        {
                            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(contentBuilder.ToString()));
                        }

                        messages.Add(assistantMessage);

                        // Then, add a new tool message for each tool call to be resolved.
                        foreach (ChatToolCall toolCall in toolCalls)
                        {
                            if (toolCall.FunctionName == "serp_search_tool")
                            {
                                string jsonParam = toolCall.FunctionArguments.ToString();
                                SerpParam searchParam = JsonSerializer.Deserialize<SerpParam>(jsonParam);
                                string toolResult = await _serpService.SearchGoogleWithString(searchParam.keywords);
                                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            }
                        }
                        requiresAction = true;
                        break;
                    default:
                        break;
                }
            }
        }
        while (requiresAction);
        Console.WriteLine("GetChatResultStreaming() leave!");
    }

    public async Task<string> GetChatTitle(string question)
    {
        await TryCreateChatClient();

        UserChatMessage userMessage = new UserChatMessage(question);
        SystemChatMessage systemMessage = new SystemChatMessage("You are a helpful assistant designed to generate concise titles for new chat conversations.  " +
            "Your sole task is to create a title describing the user's input. " +
            "**Do not attempt to answer the user's question or provide any further explanation. " +
            "**Focus on extracting the core topic of the user's query and condensing it into a short, descriptive title. " +
            "The title should be a single line in plain text in few words.");
        ChatMessage[] messages = { systemMessage, userMessage };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages);
        return RemoveThink(result.Value.Content[0].Text);
    }

    public async Task<GoogleSearchKeyword> GetGoogleSearchKeyword(string question)
    {
        await TryCreateChatClient();

        SystemChatMessage systemMessage = new SystemChatMessage("You are a helpful assistant designed to generate google search keywords. " +
            "Your sole task is to create google search keywords from user's input. " +
            "**Do not attempt to answer the user's question or provide any further explanation. " +
            "Fill the `keywords` property with keywords to search google, and `isNeedGoogleSearch` to true. " +
            "If no google search is needed, set `isNeedGoogleSearch` to false and `keywords` to null.");
        UserChatMessage userMessage = new UserChatMessage(question);
        ChatMessage[] messages = { systemMessage, userMessage };
        ChatCompletionOptions options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "google_search_keyword",
                jsonSchema: BinaryData.FromBytes("""
                    {
                        "type": "object",
                        "properties": {
                            "keywords": { 
                                "type": "string",
                                "description": "The keywords to call google search API."
                            },
                            "isNeedGoogleSearch": { 
                                "type": "boolean",
                                "description": "Is the user question need to call google search API for data or not."
                            }
                        },
                        "required": ["keywords", "isNeedGoogleSearch"],
                        "additionalProperties": false
                    }
                    """u8.ToArray()),
                jsonSchemaIsStrict: true)
        };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages, options);
        return JsonSerializer.Deserialize<GoogleSearchKeyword>(result.Value.Content[0].Text);
    }

    public async Task<string> SummarizeWebpage(string html, string query)
    {
        await TryCreateChatClient();

        SystemChatMessage systemMessage = new SystemChatMessage("You are a helpful assistant designed to summarize web pages. " +
            "Your sole task is to summarize input web page html into text, from the following context and user's query. " +
            "If given input is not able to summarize, returns the input context directly.");

        // TODO: make good token count
        if (html.Length > Constants.MAX_TOKEN + 500)
        {
            html = html.Substring(0, Constants.MAX_TOKEN + 500);
        }
        UserChatMessage userMessage = new UserChatMessage(html);
        UserChatMessage userMessageQuery = new UserChatMessage(query);
        ChatMessage[] messages = { systemMessage, userMessage, userMessageQuery };
        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages);
        return result.Value.Content[0].Text;
    }

    private static string RemoveThink(string text)
    {
        int start = text.IndexOf("<think>");
        int end = text.IndexOf("</think>") + 8;
        if (start >= 0 && end > 0)
        {
            return text.Substring(end, text.Length - end);
        }
        return text;
    }

    public async Task<float[]> GetEmbedding(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var payload = new
        {
            model = "Qwen/Qwen3-Embedding-0.6B",
            input = text,
            user = "tenny"
        };
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/v1/embeddings", payload);
            // "{"object":"error","message":"The model does not support Embeddings API","type":"BadRequestError","param":null,"code":400}"
            // "{"id":"embd-e46f57901d0b48a8951e1099df8375c7","object":"list","created":1752660182,"model":"Qwen/Qwen3-Embedding-0.6B","data":[{"index":0,"object":"embedding","embedding":[...]}]}
            EmbeddingResult result = await response.Content.ReadFromJsonAsync<EmbeddingResult>();
            if (result.code == 200 || result.code == 0)
            {
                return result.data[0].embedding;
            }
            Console.WriteLine("Error embed text: " + result.message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error embed text: " + ex);
            return null;
        }
    }

    private class SerpParam
    {
        public string keywords { get; set; }
    }

    public class StreamingChatToolCallsBuilder
    {
        private readonly Dictionary<int, string> _indexToToolCallId = [];
        private readonly Dictionary<int, string> _indexToFunctionName = [];
        private readonly Dictionary<int, SequenceBuilder<byte>> _indexToFunctionArguments = [];

        public void Append(StreamingChatToolCallUpdate toolCallUpdate)
        {
            // Keep track of which tool call ID belongs to this update index.
            if (toolCallUpdate.ToolCallId != null)
            {
                _indexToToolCallId[toolCallUpdate.Index] = toolCallUpdate.ToolCallId;
            }

            // Keep track of which function name belongs to this update index.
            if (toolCallUpdate.FunctionName != null)
            {
                _indexToFunctionName[toolCallUpdate.Index] = toolCallUpdate.FunctionName;
            }

            // Keep track of which function arguments belong to this update index,
            // and accumulate the arguments as new updates arrive.
            if (toolCallUpdate.FunctionArgumentsUpdate != null && !toolCallUpdate.FunctionArgumentsUpdate.ToMemory().IsEmpty)
            {
                if (!_indexToFunctionArguments.TryGetValue(toolCallUpdate.Index, out SequenceBuilder<byte> argumentsBuilder))
                {
                    argumentsBuilder = new SequenceBuilder<byte>();
                    _indexToFunctionArguments[toolCallUpdate.Index] = argumentsBuilder;
                }

                argumentsBuilder.Append(toolCallUpdate.FunctionArgumentsUpdate);
            }
        }

        public IReadOnlyList<ChatToolCall> Build()
        {
            List<ChatToolCall> toolCalls = [];

            foreach ((int index, string toolCallId) in _indexToToolCallId)
            {
                ReadOnlySequence<byte> sequence = _indexToFunctionArguments[index].Build();

                ChatToolCall toolCall = ChatToolCall.CreateFunctionToolCall(
                    id: toolCallId,
                    functionName: _indexToFunctionName[index],
                    functionArguments: BinaryData.FromBytes(sequence.ToArray()));

                toolCalls.Add(toolCall);
            }

            return toolCalls;
        }
    }

    public class SequenceBuilder<T>
    {
        Segment _first;
        Segment _last;

        public void Append(ReadOnlyMemory<T> data)
        {
            if (_first == null)
            {
                Debug.Assert(_last == null);
                _first = new Segment(data);
                _last = _first;
            }
            else
            {
                _last = _last!.Append(data);
            }
        }

        public ReadOnlySequence<T> Build()
        {
            if (_first == null)
            {
                Debug.Assert(_last == null);
                return ReadOnlySequence<T>.Empty;
            }

            if (_first == _last)
            {
                Debug.Assert(_first.Next == null);
                return new ReadOnlySequence<T>(_first.Memory);
            }

            return new ReadOnlySequence<T>(_first, 0, _last!, _last!.Memory.Length);
        }

        private sealed class Segment : ReadOnlySequenceSegment<T>
        {
            public Segment(ReadOnlyMemory<T> items) : this(items, 0)
            {
            }

            private Segment(ReadOnlyMemory<T> items, long runningIndex)
            {
                Debug.Assert(runningIndex >= 0);
                Memory = items;
                RunningIndex = runningIndex;
            }

            public Segment Append(ReadOnlyMemory<T> items)
            {
                long runningIndex;
                checked { runningIndex = RunningIndex + Memory.Length; }
                Segment segment = new(items, runningIndex);
                Next = segment;
                return segment;
            }
        }
    }
}

internal class EmbeddingResult
{
    public string id { get; set; }
    public uint created { get; set; }
    public string model { get; set; }
    public int code { get; set; }
    public string message { get; set; }
    public EmbeddingData[] data { get; set; }
}

internal class EmbeddingData
{
    public int index { get; set; }
    public float[] embedding { get; set; }
}

public class GoogleSearchKeyword
{
    public string keywords { get; set; }
    public bool isNeedGoogleSearch { get; set; }
}