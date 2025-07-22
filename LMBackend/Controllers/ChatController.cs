using LMBackend.Models;
using LMBackend.RAG;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Asp.Versioning;

namespace LMBackend.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class ChatController : ControllerBase
{
    private readonly ChatContext _context;
    private readonly WebScraper _scraper;
    private readonly ISerpService _serpService;
    private readonly ILlmService _llmClient;

    public ChatController(ChatContext context, ILlmService llmClient, ISerpService serpService)
    {
        _context = context;
        _scraper = new WebScraper();
        _serpService = serpService;
        _llmClient = llmClient;
    }

    // GET: api/Chat
    /// <summary>
    /// Get all chats for this user. Messages are not loaded!
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Chat>>> GetChats()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        return await _context.Chats.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedTime).ToListAsync();
    }

    // GET: api/Chat/5
    /// <summary>
    /// Get a chat and its messages by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Chat>> GetChat(Guid id)
    {
        Chat chatItem = await _context.Chats.Include(c => c.Messages.OrderBy(m => m.Timestamp)).FirstOrDefaultAsync(c => c.Id == id);

        if (chatItem == null)
        {
            return NotFound();
        }

        return Ok(chatItem);
    }

    // GET: api/Chat/{chatId}/Messages
    /// <summary>
    /// Get all messages for given chat id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}/messages")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> GetChatMessages(Guid id)
    {
        Chat chatItem = await _context.Chats.Include(c => c.Messages.OrderBy(m => m.Timestamp)).FirstOrDefaultAsync(c => c.Id == id);

        if (chatItem == null)
        {
            return NotFound();
        }

        return chatItem.Messages;
    }

    // PUT: api/Chat/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    /// <summary>
    /// Modify chat title.
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <param name="chatDto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutChat(Guid id, ChatDto chatDto)
    {
        if (id != chatDto.Id)
        {
            return BadRequest();
        }

        Chat chat = await _context.Chats.FindAsync(id);
        if (chat == null)
        {
            return NotFound(id);
        }

        chat.Title = chatDto.Title;
        _context.Entry(chat).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ChatExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Chat
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    /// <summary>
    /// Create a new chat.
    /// </summary>
    /// <param name="chatDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Chat>> PostChat(ChatDto chatDto)
    {
        // Get userId from JWT claims
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        // Check if it's a valid user
        User user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("No user in database: " + userId);
        }
        // Check if chat id exists
        if (_context.Chats.Any(x => x.Id == chatDto.Id))
        {
            return Conflict("Id: " + chatDto.Id);
        }

        Chat chat = Chat.FromDto(chatDto);
        chat.UserId = userId;

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChat), new { id = chat.Id }, chat);
    }

    // POST: api/Chat/{id}/Messages
    /// <summary>
    /// Create/append a new message, and get LLM response message.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="chatMessageDto"></param>
    /// <returns></returns>
    [HttpPost("{id:guid}/messages")]
    [Authorize]
    public async Task<ActionResult<ChatMessageResponse>> PostChatMessage(Guid id, ChatMessageDto chatMessageDto)
    {
        Chat parent = await _context.Chats.Include(c => c.Messages.OrderBy(m => m.Timestamp)).FirstOrDefaultAsync(c => c.Id == id);
        if (parent == null)
        {
            return NotFound(id);
        }

        // Ask LLM for answer
        string answer;
        try
        {
            answer = await _llmClient.GetChatResult(chatMessageDto.Text);
        }
        catch (AggregateException ex)
        {
            // 192.168.41.133 failed to connet?
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        // Generate a title for user's question, if this is the first message
        ChatDto modifiedChat = null;
        if (parent.Messages.Count == 0)
        {
            string title = await _llmClient.GetChatTitle(chatMessageDto.Text);
            //string title = "Modified title";
            modifiedChat = new ChatDto
            {
                Id = id,
                CreatedTime = parent.CreatedTime,
                Title = title,
            };
            parent.Title = title;
        }

        // Add user's and LLM result into messages
        ChatMessage chatMessage = ChatMessage.FromDto(chatMessageDto);
        chatMessage.ChatId = id;
        _context.ChatMessages.Add(chatMessage);

        ChatMessage botMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Text = answer,
            Role = Role.System,
            Timestamp = DateTime.UtcNow,
            ChatId = id,
            Model = await _llmClient.GetModelName()
        };
        _context.ChatMessages.Add(botMessage);

        // Returns the LLM message pair
        await _context.SaveChangesAsync();
        ChatMessageResponse response = new ChatMessageResponse(chatMessage, botMessage, modifiedChat);
        return Ok(response);
    }

    // POST: api/Chat/{id}/messages/stream-json
    /// <summary>
    /// Create/append a new message, and get LLM response message. Response in streaming format.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{id:guid}/messages/stream-json")]
    [Authorize]
    public async Task StreamMessageAsJson(Guid id, [FromBody] ChatMessageDto request)
    {
        // ndjson use newline to split JSON
        Response.ContentType = "application/x-ndjson";
        // Make sure headers are sent immediately
        await Response.Body.FlushAsync();
        // Let client could cancel this request
        CancellationToken ct = HttpContext.RequestAborted;
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        string modelName = await _llmClient.GetModelName();

        // Get userId from JWT claims
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            await WriteJsonStreamError(StatusCodes.Status401Unauthorized, "User id not found", id, options, modelName);
            return;
        }

        // Find parent chat id
        Chat parent = await _context.Chats
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (parent == null)
        {
            await WriteJsonStreamError(StatusCodes.Status401Unauthorized, "Chat id not found", id, options, modelName);
            return;
        }

        // Generate a title for user's question, if this is the first message
        ChatDto modifiedChat = null;
        if (parent.Messages.Count == 0)
        {
            try
            {
                string title = await _llmClient.GetChatTitle(request.Text);
                modifiedChat = new ChatDto
                {
                    Id = id,
                    CreatedTime = parent.CreatedTime,
                    Title = title,
                };
                parent.Title = title;
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await WriteJsonStreamError(StatusCodes.Status503ServiceUnavailable, "Failed to generate title.", id, options, modelName);
                return;
            }
        }

        // Save user message
        ChatMessage userMessage = ChatMessage.FromDto(request);
        userMessage.ChatId = id;
        _context.ChatMessages.Add(userMessage);


        // Create empty bot message to hold streaming data
        int index = 0;
        StringBuilder botReplyBuilder = new();
        ChatMessage botMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatId = id,
            Text = string.Empty,
            Role = Role.System,
            Timestamp = DateTime.UtcNow,
            Model = modelName
        };

        // Do RAG if user asked
        string ragResult = null;
        if (request.UseRetrieval)
        {
            // Return a status text
            string statusText = "*Doing RAG search...*  \n";
            botReplyBuilder.Append(statusText);
            await WriteJsonStreamProgress(statusText, index, id, botMessage, userMessage.Id, options, modelName);
            index++;

            // 1. Generate embedding for user query
            float[] embedding = await _llmClient.GetEmbedding(request.Text);
            if (embedding == null)
            {
                await WriteJsonStreamError(StatusCodes.Status503ServiceUnavailable, "Failed to get embeddings.", id, options, modelName);
                return;
            }

            // 2. Query ChromaDB with embedding (REST)
            // Get collection id for this user
            string collectionId = await ChromaVectorStoreService.Instance.TryCreateCollection(userId);
            // Search for the text
            IList<ChromaRagChunkResult> chunkResult = await ChromaVectorStoreService.Instance.SearchAsync(collectionId, parent.Id, embedding, 5, null);
            if (chunkResult == null)
            {
                await WriteJsonStreamError(StatusCodes.Status503ServiceUnavailable, "Failed to search ChromaDB.", id, options, modelName);
                return;
            }

            // 3. Build prompt: concat retrieved chunks + user query
            ragResult = string.Join("\n", chunkResult);
        }

        // Do web search
        if (request.UseWebSearch)
        {
            // Return a status text
            string statusText = "*Doing web search...*  \n";
            botReplyBuilder.Append(statusText);
            await WriteJsonStreamProgress(statusText, index, id, botMessage, userMessage.Id, options, modelName);
            index++;

            // Search for web
            GoogleSearchKeyword gk = await _llmClient.GetGoogleSearchKeyword(request.Text);
            if (gk.isNeedGoogleSearch)
            {
                SerpResultSchema searchResult = await _serpService.SearchGoogle(gk.keywords);
                if (searchResult != null && searchResult.organic_results.Length > 0)
                {
                    // Summarize the json to text
                    foreach (SerpOrganicResult o in searchResult.organic_results)
                    {
                        ragResult += "\n" + JsonSerializer.Serialize(o);

                        // Tell client what we have searched
                        string searchedSite = $" - <a href=\"{o.link}\" target=\"_blank\">{o.title}</a>  \n";
                        botReplyBuilder.Append(searchedSite);
                        await WriteJsonStreamProgress(searchedSite, index, id, botMessage, userMessage.Id, options, modelName);
                        index++;
                    }
                }
                /*List<GoogleSearchResult> searchResults = await GoogleCustomSearchService.Instance.SearchAsync(gk.keywords, 5);
                if (searchResults != null && searchResults.Count > 0)
                {
                    // Get content from URL
                    foreach (GoogleSearchResult searchResult in searchResults)
                    {
                        string html = await _scraper.Scrap(searchResult.link);
                        if (string.IsNullOrEmpty(html))
                        {
                            Console.WriteLine("HTML is empty for link: " + searchResult.link);
                            continue;
                        }
                        Console.WriteLine(html);

                        // Summarize the html to text
                        string content;
                        try
                        {
                            content = await _llmClient.SummarizeWebpage(html, request.Text);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to generate summary for:" + searchResult.link + "\nError: " + ex.Message);
                            continue;
                        }

                        // Create prompt
                        ragResult += $"\nURL: {searchResult.formattedUrl}\nTitle: {searchResult.title}\nContent: {content}\n";

                        // Tell client what we have searched
                        //string searchedSite = $" - [{searchResult.title}]({searchResult.link})\n";
                        string searchedSite = $" - <a href=\"{searchResult.link}\" target=\"_blank\">{searchResult.title}</a>  \n";
                        botReplyBuilder.Append(searchedSite);
                        await WriteJsonStreamProgress(searchedSite, index, id, botMessage, userMessage.Id, options);
                        index++;
                    }

                    // Append a linebreak after list
                    botReplyBuilder.Append("\n");
                    await WriteJsonStreamProgress("\n", index, id, botMessage, userMessage.Id, options);
                    index++;
                }*/
                else
                {
                    await WriteJsonStreamError(StatusCodes.Status503ServiceUnavailable, "Failed to search Google.", id, options, modelName);
                    return;
                }
            }
        }

        // Call streaming endpoint
        try
        {
            IAsyncEnumerable<string> streamingTexts = _llmClient.GetChatResultStreaming(parent.Messages, request.Text, ragResult);
            await foreach (string part in streamingTexts.WithCancellation(ct))
            {
                botReplyBuilder.Append(part);
                await WriteJsonStreamProgress(part, index, id, botMessage, userMessage.Id, options, modelName);
                index++;
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex);
            await WriteJsonStreamError(StatusCodes.Status503ServiceUnavailable, "Failed to generate LLM response.", id, options, modelName);
            return;
        }

        // Save the botMessage's full text and userMessage
        botMessage.Text = botReplyBuilder.ToString();
        _context.ChatMessages.Add(botMessage);
        await _context.SaveChangesAsync();

        // Signal the end of the stream
        // We don't have text in the final message, but we still need to send a completion signal
        ChatMessageStreamResponse done = new ChatMessageStreamResponse
        {
            ChatId = id,
            MessageId = botMessage.Id,
            ReplyMessageId = userMessage.Id,
            Sequence = index,  // Tell client the total length of the message sent
            Text = string.Empty,
            Model = modelName,
            Status = StreamStatus.Completed,
            Timestamp = botMessage.Timestamp,
            ChatModified = modifiedChat
        };

        await Response.WriteAsync(JsonSerializer.Serialize(done, options) + "\n", ct);
        await Response.Body.FlushAsync();
    }

    // DELETE: api/Chat/5
    /// <summary>
    /// Delete a chat by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteChat(Guid id)
    {
        var chatItem = await _context.Chats.FindAsync(id);
        if (chatItem == null)
        {
            return NotFound();
        }

        _context.Chats.Remove(chatItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ChatExists(Guid id)
    {
        return _context.Chats.Any(e => e.Id == id);
    }

    private async Task WriteJsonStreamProgress(string part, int index, Guid chatId, ChatMessage botMessage, Guid replyMessagId, JsonSerializerOptions options, string modelName)
    {
        ChatMessageStreamResponse chunk = new ChatMessageStreamResponse
        {
            ChatId = chatId,
            MessageId = botMessage.Id,
            ReplyMessageId = replyMessagId,
            Sequence = index,
            Text = part,
            Model = modelName,
            Status = StreamStatus.InProgress,
            Timestamp = botMessage.Timestamp
        };

        string json = JsonSerializer.Serialize(chunk, options);
        await Response.WriteAsync(json + "\n");
        await Response.Body.FlushAsync();
    }

    private async Task WriteJsonStreamError(int statusCode, string errorMessage, Guid chatId, JsonSerializerOptions options, string modelName)
    {
        ChatMessageStreamResponse chunk = new ChatMessageStreamResponse
        {
            ChatId = chatId,
            Model = modelName,
            Status = StreamStatus.Failed,
            Timestamp = DateTime.UtcNow,
            Error = errorMessage
        };
        string json = JsonSerializer.Serialize(chunk, options);
        await Response.WriteAsync(json + "\n");
        await Response.Body.FlushAsync();
        //Response.StatusCode = statusCode;  // Cannot set StatusCode after writing to body!
    }
}
