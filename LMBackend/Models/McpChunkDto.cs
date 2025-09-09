using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace LMBackend.Models;

public class McpServerChunkDto
{
    /// <summary>
    /// Name of the MCP server
    /// </summary>
    [JsonPropertyName("server_name")]
    public string ServerName { get; set; }

    /// <summary>
    /// MCP server type (stdio/sse/streamableHttp)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// MCP server source (local/cloud)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// MCP tool array
    /// </summary>
    public List<McpTool> Tools { get; set; }

    public override string ToString()
    {
        string result = $"ServerName: {ServerName},\ntype: {Type}\nsource: {Source}";
        if (Tools != null)
        {
            result += ",\nTools: [\n" + string.Join(",\n", Tools) + "\n]";
        }
        return result;
    }
}

public class McpTool
{
    /// <summary>
    /// Name of the MCP tool
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Description of the MCP tool
    /// </summary>
    public string Description { get; set; }

    public JsonNode InputSchema { get; set; }

    public JsonNode OutputSchema { get; set; }

    /// <summary>
    /// Annotations of the MCP tool (could be null)
    /// </summary>
    public McpToolAnnotation Annotations { get; set; }

    public override string ToString()
    {
        string result = $"name: {Name},\tdescription: {Description}";
        if (InputSchema != null)
        {
            result += $",\tinputSchema: {InputSchema.ToString()}";
        }
        if (OutputSchema != null)
        {
            result += $",\toutputSchema: {OutputSchema.ToString()}";
        }
        if (Annotations != null)
        {
            result += $",\tannotations: {Annotations.ToString()}";
        }
        return result;
    }
}

public class McpToolAnnotation
{
    public string Title { get; set; }
    public bool ReadOnlyHint { get; set; }
    public bool DestructiveHint { get; set; }
    public bool OpenWorldHint { get; set; }

    public override string ToString()
    {
        return $"Title: {Title}, ReadOnly: {ReadOnlyHint}, Destructive: {DestructiveHint}, OpenWorld: {OpenWorldHint}";
    }
}