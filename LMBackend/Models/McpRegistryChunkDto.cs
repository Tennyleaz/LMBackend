namespace LMBackend.Models;

public class McpRegistryChunkDto
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public override string ToString()
    {
        string result = $"Name: {Name},\nTitle: {Title}\nDescription: {Description}";
        return result;
    }
}
