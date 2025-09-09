namespace LMBackend.Models;

public class DocumentChunkDto
{
    /// <summary>
    /// Name of the document
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// List of the document text/MCP tool
    /// </summary>
    public List<DocuemtChunkItem> Chunks { get; set; }
}

public class DocuemtChunkItem
{
    /// <summary>
    /// Name of the MCP tool
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Category of the MCP tool (could be null)
    /// </summary>
    public string Category { get; set; }
    /// <summary>
    /// Description of the MCP tool
    /// </summary>
    public string Description { get; set; }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Category))
            return $"name: {Name},\ndescription: {Description}";
        return $"name: {Name},\ncategory: {Category}\n, description: {Description}";
    }
}