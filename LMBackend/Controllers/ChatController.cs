using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMBackend.Models;
using System.Text.Json;
using System.Diagnostics;

namespace LMBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly ChatContext _context;

    public ChatController(ChatContext context)
    {
        _context = context;
    }

    // GET: api/Chat
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Chat>>> GetChats()
    {
        return await _context.Chats.ToListAsync();
    }

    // GET: api/Chat/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Chat>> GetChat(Guid id)
    {
        var chatItem = await _context.Chats.FindAsync(id);

        if (chatItem == null)
        {
            return NotFound();
        }

        return chatItem;
    }

    // GET: api/Chat/{chatId}/Messages
    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<List<ChatMessage>>> GetChatMessages(Guid id)
    {
        var chatItem = await _context.Chats.FindAsync(id);

        if (chatItem == null)
        {
            return NotFound();
        }

        return chatItem.Messages;
    }

    // PUT: api/Chat/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
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
    [HttpPost]
    public async Task<ActionResult<Chat>> PostChat(Chat chatItem)
    {
        _context.Chats.Add(chatItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetChatItem", new { id = chatItem.Id }, chatItem);
    }

    // POST: api/Chat/{id}/Messages
    [HttpPost("{id:guid}/messages")]
    public async Task<ActionResult<ChatMessage>> PostChatMessage(Guid id, ChatMessage chatMessage)
    {
        Chat parent = await _context.Chats.FindAsync(id);
        if (parent == null)
        {
            return NotFound(parent.Id);
        }

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetChat), new { id = chatMessage.Id }, chatMessage);
    }

    // POST: api/Chat/{id}/messages/stream-json
    [HttpPost("{id:guid}/messages/stream-json")]
    public async Task StreamMessageAsJson(Guid id, [FromBody] ChatMessage request)
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
        chat.Messages.Add(request);

        // Call chatbot service (mock or OpenAI, etc.)
        ChatMessage botMessage = new ChatMessage
        {            
            Id = Guid.NewGuid(),
            Text = "This is a simulated bot response to: " + request.Text,
            Role = Role.System,
            Timestamp = DateTimeOffset.UtcNow
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
    [HttpDelete("{id}")]
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
