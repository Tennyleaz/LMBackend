using LMBackend.Models;

namespace LMBackend;

public class ChromaVectorStoreService
{
    private readonly HttpClient _httpClient;
    private readonly ChromaClient _chromaClient;
    private const string TENANT = "tenny";
    private const string DATABASE = "tennydb";

    public static ChromaVectorStoreService Instance { get; private set; }

    private ChromaVectorStoreService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(Constants.CHROMA_ENDPOINT);
        _chromaClient = new ChromaClient(Constants.CHROMA_ENDPOINT, _httpClient);
    }

    public static void TryCreateChromaInstance()
    {
        if (Instance != null)
            return;
        Instance = new ChromaVectorStoreService();
    }

    /// <summary>
    /// Create database from user id, if not created.
    /// </summary>
    /// <returns>Returns the databse name (=user id)</returns>
    public async Task<string> TryCreateDatabaseForUser()
    {
        Database userDb = null;
        try
        {
            userDb = await _chromaClient.Get_databaseAsync(TENANT, DATABASE);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Get database error: " + ex.Message);
        }
        if (userDb != null)
        {
            return userDb.Name;
        }
        else
        {
            try
            {
                CreateDatabaseResponse response = await _chromaClient.Create_databaseAsync(TENANT, new CreateDatabasePayload
                {
                    Name = DATABASE,
                });
                return DATABASE;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create database error: " + ex.Message);
            }
            return null;
        }
    }

    public async Task<string> TryCreateCollection(Guid userId)
    {
        string collectionName = userId + "_collection";
        // Get first
        try
        {
            ICollection<Collection> collections = await _chromaClient.List_collectionsAsync(TENANT, DATABASE, null, null);
            Collection collection = collections.FirstOrDefault(x => x.Name == collectionName);
            if (collection != null)
            {
                return collection.Id.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Get collection error: " + ex);
        }

        try
        {
            CreateCollectionPayload body = new CreateCollectionPayload
            {
                Name = collectionName,
            };
            Collection response = await _chromaClient.Create_collectionAsync(TENANT, DATABASE, body);
            return response.Id.ToString();
        }
        catch (ApiException ex)
        {
            Console.WriteLine("Create collection error: " + ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Create collection error: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Upserts records in a collection (create if not exists, otherwise update).
    /// </summary>
    /// <returns>Returns upsert success/fail.</returns>
    public async Task<bool> UpsertAsync(string collectionId, List<ChromaChunk> chunks)
    {
        if (string.IsNullOrEmpty(collectionId))
            return false;

        UpsertCollectionRecordsPayload payload = new UpsertCollectionRecordsPayload
        {
            Documents = chunks.Select(x => x.Document).ToList(),
            Embeddings = chunks.Select(x => x.Embedding).ToList(),
            Ids = chunks.Select(x => x.Id).ToList(),
            Metadatas = chunks.Select(x => x.Metadata).ToList()
        };
        try
        {
            await _chromaClient.Collection_upsertAsync(TENANT, DATABASE, collectionId, payload);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Upsert collection error: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Search records in a collection by embeddings.
    /// </summary>
    /// <returns>Returns the search document list. Null if fail.</returns>
    public async Task<IList<ChromaRagChunkResult>> SearchAsync(string collectionId, Guid chatId, float[] embedding, int? topK, int? offset)
    {
        if (string.IsNullOrEmpty(collectionId))
            return null;

        // Prepare query
        var payload = new QueryRequestPayload
        {
            Query_embeddings = { embedding },
            N_results = topK,
            Include = new Include[] { Include.Distances, Include.Documents, Include.Metadatas },
            Ids = null  // Important! Empty array returns nothing!
        };
        if (chatId != Guid.Empty)
        {
            // Create filter by chat id
            payload.Where = new Dictionary<string, object>
            {
                { "chatId", chatId.ToString() },
            };
        }
        else
        {
            // No filter
            payload.Where = null;
        }

        try
        {
            QueryResponse response = await _chromaClient.Collection_queryAsync(TENANT, DATABASE, collectionId, topK, offset, payload);
            List<ChromaRagChunkResult> chunkResult = ChromaRagChunkResult.FromQueryResponse(response);
            return chunkResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Search collection error: " + ex.Message);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string collectionId, Guid documentId)
    {
        if (string.IsNullOrEmpty(collectionId))
            return false;

        try
        {
            // Find records from document id in metadata
            DeleteCollectionRecordsResponse deleteResponse = await _chromaClient.Collection_deleteAsync(TENANT, DATABASE, collectionId, new DeleteCollectionRecordsPayload
            {
                Where = new Dictionary<string, object>
                {
                    { "documentId", documentId.ToString() }
                },
                Ids = null  // Important! Empty array returns nothing!
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Delete collection error: " + ex.Message);
            return false;
        }
    }
}
