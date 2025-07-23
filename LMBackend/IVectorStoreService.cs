using LMBackend.RAG;

namespace LMBackend;

public interface IVectorStoreService
{
    public Task<string> TryCreateDatabaseForUser();
    public Task<string> TryCreateCollection(Guid userId);
    public Task<bool> UpsertAsync(string collectionId, List<ChromaChunk> chunks);
    public Task<IList<ChromaRagChunkResult>> SearchAsync(string collectionId, Guid chatId, float[] embedding, int? topK, int? offset);
    public Task<bool> DeleteAsync(string collectionId, Guid documentId);
}
