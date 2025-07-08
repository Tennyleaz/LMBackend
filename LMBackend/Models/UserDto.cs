namespace LMBackend.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int ChatCount { get; set; }

    public static UserDto FromUser(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            ChatCount = user.Chats?.Count ?? 0
        };
    }
}
