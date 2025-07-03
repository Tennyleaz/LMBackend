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
    public async Task<ActionResult<IEnumerable<ChatItem>>> GetChatItems()
    {
        return await _context.ChatItems.ToListAsync();
    }

    // GET: api/Chat/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ChatItem>> GetChatItem(Guid id)
    {
        var chatItem = await _context.ChatItems.FindAsync(id);

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
        var chatItem = await _context.ChatItems.FindAsync(id);

        if (chatItem == null)
        {
            return NotFound();
        }

        return chatItem.Messages;
    }

    // PUT: api/Chat/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutChatItem(Guid id, ChatItem chatItem)
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
            if (!ChatItemExists(id))
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
    public async Task<ActionResult<ChatItem>> PostChatItem(ChatItem chatItem)
    {
        _context.ChatItems.Add(chatItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetChatItem", new { id = chatItem.Id }, chatItem);
    }

    // POST: api/Chat/{id}/Message
    [HttpPost("{id:guid}/messages")]
    public async Task<ActionResult<ChatMessage>> PostChatMessage(Guid id, ChatMessage chatMessage)
    {
        ChatItem parent = await _context.ChatItems.FindAsync(id);
        if (parent == null)
        {
            return NotFound(parent.Id);
        }

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetChatItem), new { id = chatMessage.Id }, chatMessage);
    }

    // DELETE: api/Chat/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChatItem(Guid id)
    {
        var chatItem = await _context.ChatItems.FindAsync(id);
        if (chatItem == null)
        {
            return NotFound();
        }

        _context.ChatItems.Remove(chatItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ChatItemExists(Guid id)
    {
        return _context.ChatItems.Any(e => e.Id == id);
    }
}
