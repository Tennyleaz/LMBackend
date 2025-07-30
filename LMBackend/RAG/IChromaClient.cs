using LMBackend.RAG.Chroma;
using System.Runtime.CompilerServices;

namespace LMBackend.RAG;

public interface IChromaClient
{
    // We only add methods that are really used here

    Task<Database> Get_databaseAsync(string tenant, string database);
    Task<CreateDatabaseResponse> Create_databaseAsync(string tenant, CreateDatabasePayload body);
    Task<ICollection<Collection>> List_collectionsAsync(string tenant, string database, int? limit, int? offset);
    Task<Collection> Create_collectionAsync(string tenant, string database, CreateCollectionPayload body);
    Task<UpsertCollectionRecordsResponse> Collection_upsertAsync(string tenant, string database, string collection_id, UpsertCollectionRecordsPayload body);
    Task<QueryResponse> Collection_queryAsync(string tenant, string database, string collection_id, int? limit, int? offset, QueryRequestPayload body);
    Task<DeleteCollectionRecordsResponse> Collection_deleteAsync(string tenant, string database, string collection_id, DeleteCollectionRecordsPayload body);
}
