namespace LMBackend.Models;

public class DocumentDto
{
    public string Name { get; set; }    
    public Guid ChatId { get; set; }
    public DateTime CreatedTime { get; set; }
    public byte[] Data { get; set; }
}
