using LMBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace LMBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly ChatContext _context;
    private readonly LlmClient _llmClient;

    public ChatController(ChatContext context)
    {
        _context = context;
        _llmClient = new LlmClient(Constants.LLM_KEY, Constants.MODEL);
    }

    // GET: api/Chat
    /// <summary>
    /// Get all chats for this user.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Chat>>> GetChats()
    {
        return await _context.Chats.ToListAsync();
    }

    // GET: api/Chat/5
    /// <summary>
    /// Get a chat by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Chat>> GetChat(Guid id)
    {
        var chatItem = await _context.Chats.Include(c => c.Messages.OrderBy(m => m.Timestamp)).FirstOrDefaultAsync(c => c.Id == id);

        if (chatItem == null)
        {
            return NotFound();
        }

        return chatItem;
    }

    // GET: api/Chat/{chatId}/Messages
    /// <summary>
    /// Get all messages for given chat id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}/messages")]
    [Authorize]
    public async Task<ActionResult<List<ChatMessage>>> GetChatMessages(Guid id)
    {
        var chatItem = await _context.Chats.Include(c => c.Messages.OrderBy(m => m.Timestamp)).FirstOrDefaultAsync(c => c.Id == id);

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
    public async Task<IActionResult> PutChat(Guid id, Chat chatItem)
    {
        if (id != chatItem.Id)
        {
            return BadRequest();
        }

        _context.Entry(chatItem).State = EntityState.Modified;

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
        Claim userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub"); // Try standard and JWT 'sub'
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        // Check if chat id exists
        if (_context.Chats.Any(x => x.Id == chatDto.Id))
        {
            return Conflict("Id: " + chatDto.Id);
        }

        Guid userId = Guid.Parse(userIdClaim.Value);
        Chat chat = Chat.FromDto(chatDto);
        chat.UserId = userId;
        //chat.User = user;

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
    public async Task<ActionResult<ChatMessage>> PostChatMessage(Guid id, ChatMessageDto chatMessageDto)
    {
        Chat parent = await _context.Chats.FindAsync(id);
        if (parent == null)
        {
            if (Debugger.IsAttached)
            {
                parent = new Chat
                {
                    Id = id,
                    Title = "New Chat",
                    Messages = new List<ChatMessage>()
                };
            }
            else
            {
                return NotFound(parent.Id);
            }
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

        // Add user's and LLM result into messages
        ChatMessage chatMessage = ChatMessage.FromDto(chatMessageDto);
        //chatMessage.Chat = parent;
        chatMessage.ChatId = id;
        _context.ChatMessages.Add(chatMessage);

        ChatMessage botMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Text = answer,
            Role = Role.System,
            Timestamp = DateTimeOffset.UtcNow,
            Chat = parent,
            ChatId = id,
        };
        _context.ChatMessages.Add(botMessage);

        // Returns the LLM message
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetChat), new { id = botMessage.Id }, botMessage);
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

        // Find parent chat id
        Chat chat = await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chat == null)
        {
            if (Debugger.IsAttached)
            {
                chat = new Chat
                {
                    Id = id,
                    Title = "New Chat",
                    Messages = new List<ChatMessage>()
                };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                await Response.WriteAsync(JsonSerializer.Serialize(new { error = "Chat not found" }) + "\n");
                await Response.Body.FlushAsync();
                return;
            }
        }

        // Save user message
        ChatMessage chatMessage = ChatMessage.FromDto(request);
        //chatMessage.Chat = chat;
        chatMessage.ChatId = id;
        chat.Messages.Add(chatMessage);

        // Call chatbot service (mock or OpenAI, etc.)
        ChatMessage botMessage = new ChatMessage
        {            
            Id = Guid.NewGuid(),
            ChatId = id,
            Text = "This is a simulated bot response to: " + request.Text,
            Role = Role.System,
            Timestamp = DateTime.UtcNow
        };
        chat.Messages.Add(botMessage);

        // Simulate streaming a chatbot reply
        string reply = botMessage.Text;
        for (int i = 0; i < reply.Length; i++)
        {
            ChatMessageStreamResponse chunk = new ChatMessageStreamResponse
            {
                ChatId = id,
                MessageId = botMessage.Id,
                Sequence = i,
                Text = reply[i].ToString(),
                Status = StreamStatus.InProgress,
                Timestamp = botMessage.Timestamp
            };

            string json = JsonSerializer.Serialize(chunk);
            await Response.WriteAsync(json + "\n");
            await Response.Body.FlushAsync();
            await Task.Delay(50); // simulate delay
        }

        // Signal the end of the stream
        // We don't have text in the final message, but we still need to send a completion signal
        ChatMessageStreamResponse done = new ChatMessageStreamResponse
        {
            ChatId = id,
            MessageId = botMessage.Id,
            Sequence = reply.Length,  // Tell client the total length of the message sent
            Text = string.Empty,
            Status = StreamStatus.Completed,
            Timestamp = botMessage.Timestamp
        };

        await Response.WriteAsync(JsonSerializer.Serialize(done) + "\n");
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
}
