using LMBackend.RAG.Chroma;

namespace LMBackend.RAG;

public class ChromaRagChunkResult
{
    public string Id { get; set; }
    public string Document { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public float? Distance { get; set; }

    public static List<ChromaRagChunkResult> FromQueryResponse(QueryResponse chromaResp)
    {
        var docs = chromaResp.Documents.FirstOrDefault()?.ToArray();
        var ids = chromaResp.Ids.FirstOrDefault()?.ToArray();
        var metas = chromaResp.Metadatas.FirstOrDefault()?.ToArray();
        var dists = chromaResp.Distances.FirstOrDefault()?.ToArray();

        var results = new List<ChromaRagChunkResult>();
        for (int i = 0; i < ids.Length; i++)
        {
            results.Add(new ChromaRagChunkResult
            {
                Id = ids[i],
                Document = docs[i],
                Metadata = metas[i],
                Distance = dists[i]
            });
        }
        return results;
    }

    public override string ToString()
    {
        string result = "";
        if (Distance.HasValue)
            result += $"###Distance: {Distance}\n\n";
        result += $"###Document Name: {Metadata["documentName"]}\n\n";        
        result += $"###Document Chunk: {Document}\n\n";
        result += $"###Document Chunk Index: {Metadata["chunkIndex"]} ";
        return result;
    }
}
