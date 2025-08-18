namespace LMBackend.Models;

public class DocumentSearchResponse
{
    public string Id { get; set; }
    public string Document { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public float? Distance { get; set; }
}
