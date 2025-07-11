namespace LMBackend.Models;

public class LlmModel
{
    public LlmModel(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; set; }
    public string Description { get; set; }
}
