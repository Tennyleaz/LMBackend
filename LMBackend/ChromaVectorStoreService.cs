namespace LMBackend;

public class ChromaVectorStoreService
{
    private readonly HttpClient _httpClient;
    private readonly ChromaClient _chromaClient;
    private const string TENANT = "tenny";
    private const string DATABASE = "tennydb";

    public ChromaVectorStoreService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(Constants.CHROMA_ENDPOINT);
        _chromaClient = new ChromaClient(Constants.CHROMA_ENDPOINT, _httpClient);
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

    public async Task<string> TryCreateCollection(string userId)
    {
        string collectionId = userId + "_collection";
        try
        {
            AddCollectionRecordsResponse response = await _chromaClient.Collection_addAsync(TENANT, DATABASE, collectionId, new AddCollectionRecordsPayload
            {

            });
            return collectionId;
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
    public async Task<bool> UpsertAsync(string userId, List<ChromaChunk> chunks)
    {
        string collectionId = userId + "_collection";
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
    public async Task<IList<string>> SearchAsync(string userId, string chatId, float[] embedding, int? topK, int? offset)
    {
        string collectionId = userId + "_collection";
        try
        {
            QueryResponse response = await _chromaClient.Collection_queryAsync(TENANT, DATABASE, collectionId, topK, offset, new QueryRequestPayload
            {
                Query_embeddings = { embedding },
                N_results = topK,
                Where = new Dictionary<string, object>
                {
                    { "chatId", chatId },
                    //{ "userId", userId }
                },
                Include = new Include[] { Include.Distances, Include.Documents, Include.Metadatas }
            });
            return response.Documents.FirstOrDefault()?.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Search collection error: " + ex.Message);
            return null;
        }
    }
}
