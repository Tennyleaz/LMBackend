namespace LMBackend.Models;

public class DocumentSearchRequest
{
    public string Query { get; set; }
    public int TopK { get; set; }
}

