using Microsoft.EntityFrameworkCore;

namespace LMBackend.Models;

public class ChatContext : DbContext
{
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {

    }

    public DbSet<ChatItem> ChatItems { get; set; } = null;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null;
}
