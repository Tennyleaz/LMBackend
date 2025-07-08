using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMBackend.Models;

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

    // POST: api/Chat/{id}/Message
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
