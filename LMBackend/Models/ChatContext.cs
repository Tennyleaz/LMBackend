using Microsoft.EntityFrameworkCore;

namespace LMBackend.Models;

public class ChatContext : DbContext
{
    public ChatContext(DbContextOptions<ChatContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; } = null;
    public DbSet<Chat> Chats { get; set; } = null;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null;
    public DbSet<Document> Documents { get; set; } = null;
}
