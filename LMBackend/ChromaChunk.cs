namespace LMBackend;

public class ChromaChunk
{
    public string Id { get; set; }
    public string Document { get; set; }  // The full chunked text
    public float[] Embedding { get; set; }
    public Dictionary<string, object> Metadata { get; set; }  // Contains userId, chatId, documentId, chunkIndex, ...

    public ChromaChunk(Guid userId, Guid chatId, Guid documentId, int documentIndex, string documentChunkText, float[] embedding)
    {
        // name the id from user + chat + doc
        //Id = $"{userId}_{chatId}_{documentId}_{documentIndex}";
        Id = Guid.NewGuid().ToString();
        Document = documentChunkText;
        Embedding = embedding;
        Metadata = new Dictionary<string, object>();
        Metadata.Add("userId", userId.ToString());
        Metadata.Add("chatId", chatId.ToString());
        Metadata.Add("documentId", documentId.ToString());
        Metadata.Add("chunkIndex", documentIndex);
    }
}
