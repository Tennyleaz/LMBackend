namespace LMBackend.Models;

public class DocumentChunkDto
{
    public string Name { get; set; }
    public List<DocuemtChunkItem> Chunks { get; set; }
}

public class DocuemtChunkItem
{
    public string Name { get; set; }
    public string Description { get; set; }

    public override string ToString()
    {
        return $"name: {Name},\n description: {Description}";
    }
}
