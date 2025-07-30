using LMBackend.RAG.Chroma;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMBackend.RAG.Tests;

[TestFixture()]
public class ChromaVectorStoreServiceTests
{
    private Mock<IChromaClient> _chromaClient;
    private ChromaVectorStoreService _service;

    [SetUp]
    public void Setup()
    {
        _chromaClient = new Mock<IChromaClient>();
        _service = new ChromaVectorStoreService(_chromaClient.Object);
    }

    [Test()]
    public async Task TryCreateDatabaseForUser_TestExist()
    {
        // Arrange
        Database db = new Database
        {
            Name = "db",
            Id = Guid.NewGuid(),
            Tenant = "",
            AdditionalProperties = null
        };
        // Mock create existing db
        _chromaClient.Setup(x => x.Get_databaseAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(db);

        // Act
        string name = await _service.TryCreateDatabaseForUser();

        // Assert
        Assert.That(name, Is.EqualTo(db.Name));
    }

    [Test()]
    public async Task TryCreateDatabaseForUser_TestCreateOk()
    {
        // Arrange
        Database db = new Database
        {
            Name = "tennydb",
            Id = Guid.NewGuid(),
            Tenant = "",
            AdditionalProperties = null
        };
        // Mock return null
        _chromaClient.Setup(x => x.Get_databaseAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null as Database);
        // Mock create new db
        _chromaClient.Setup(x => x.Create_databaseAsync(It.IsAny<string>(), It.IsAny<CreateDatabasePayload>())).ReturnsAsync(new CreateDatabaseResponse());

        // Act
        string name = await _service.TryCreateDatabaseForUser();

        // Assert
        Assert.That(name, Is.EqualTo(db.Name));
    }

    [Test()]
    public async Task TryCreateDatabaseForUser_TestCreateException()
    {
        // Arrange
        Database db = new Database
        {
            Name = "tennydb",
            Id = Guid.NewGuid(),
            Tenant = "",
            AdditionalProperties = null
        };
        // Mock return null
        _chromaClient.Setup(x => x.Get_databaseAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null as Database);
        // Mock create new db
        _chromaClient.Setup(x => x.Create_databaseAsync(It.IsAny<string>(), It.IsAny<CreateDatabasePayload>())).Throws(new Exception("Mock exception"));

        // Act
        string name = await _service.TryCreateDatabaseForUser();

        // Assert
        Assert.That(name, Is.EqualTo(null));
    }

    [Test()]
    public async Task TryCreateDatabaseForUser_TestGetException()
    {
        // Arrange
        Database db = new Database
        {
            Name = "tennydb",
            Id = Guid.NewGuid(),
            Tenant = "",
            AdditionalProperties = null
        };
        // Mock exception
        _chromaClient.Setup(x => x.Get_databaseAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Mock exception"));
        // Mock create new db
        _chromaClient.Setup(x => x.Create_databaseAsync(It.IsAny<string>(), It.IsAny<CreateDatabasePayload>())).ReturnsAsync(new CreateDatabaseResponse());

        // Act
        string name = await _service.TryCreateDatabaseForUser();

        // Assert
        Assert.That(name, Is.EqualTo(db.Name));
    }

    [Test()]
    public async Task TryCreateCollection_TestExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string collectionName = userId + "_collection";
        Collection newCollection = new Collection
        {
            Name = collectionName,
            Database = "db",
            Tenant = "tenny",
            Id = Guid.NewGuid(),
            Dimension = 1024,
            Log_position = 0,
            Metadata = new Dictionary<string, bool>(),
            Version = 1,
            AdditionalProperties = null
        };
        List<Collection> fakeCollections = new List<Collection> { newCollection };
        // Mock return collection
        _chromaClient.Setup(x => x.List_collectionsAsync(It.IsAny<string>(), It.IsAny<string>(), null, null)).ReturnsAsync(fakeCollections);

        // Act
        string collectionId = await _service.TryCreateCollection(userId);

        // Assert
        Assert.That(collectionId, Is.EqualTo(newCollection.Id.ToString()));
    }

    [Test()]
    public async Task TryCreateCollection_TestCreateOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string collectionName = userId + "_collection";
        Collection newCollection = new Collection
        {
            Name = collectionName,
            Database = "db",
            Tenant = "tenny",
            Id = Guid.NewGuid(),
            Dimension = 1024,
            Log_position = 0,
            Metadata = new Dictionary<string, bool>(),
            Version = 1,
            AdditionalProperties = null
        };
        List<Collection> fakeCollections = new List<Collection> { newCollection };
        // Mock return empty list
        _chromaClient.Setup(x => x.List_collectionsAsync(It.IsAny<string>(), It.IsAny<string>(), null, null)).ReturnsAsync(new List<Collection>());
        // Mock create collection
        _chromaClient.Setup(x => x.Create_collectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CreateCollectionPayload>())).ReturnsAsync(newCollection);

        // Act
        string collectionId = await _service.TryCreateCollection(userId);

        // Assert
        Assert.That(collectionId, Is.EqualTo(newCollection.Id.ToString()));
    }

    [Test()]
    public async Task TryCreateCollection_TestCreateApiException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string collectionName = userId + "_collection";
        Collection newCollection = new Collection
        {
            Name = collectionName,
            Database = "db",
            Tenant = "tenny",
            Id = Guid.NewGuid(),
            Dimension = 1024,
            Log_position = 0,
            Metadata = new Dictionary<string, bool>(),
            Version = 1,
            AdditionalProperties = null
        };
        List<Collection> fakeCollections = new List<Collection> { newCollection };
        // Mock return empty list
        _chromaClient.Setup(x => x.List_collectionsAsync(It.IsAny<string>(), It.IsAny<string>(), null, null)).ReturnsAsync(new List<Collection>());
        // Mock create collection exception
        _chromaClient.Setup(x => x.Create_collectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CreateCollectionPayload>())).Throws(new Exception("Mock exception"));

        // Act
        string collectionId = await _service.TryCreateCollection(userId);

        // Assert
        Assert.That(collectionId, Is.EqualTo(null));
    }

    [Test()]
    public async Task TryCreateCollection_TestCreateException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string collectionName = userId + "_collection";
        Collection newCollection = new Collection
        {
            Name = collectionName,
            Database = "db",
            Tenant = "tenny",
            Id = Guid.NewGuid(),
            Dimension = 1024,
            Log_position = 0,
            Metadata = new Dictionary<string, bool>(),
            Version = 1,
            AdditionalProperties = null
        };
        List<Collection> fakeCollections = new List<Collection> { newCollection };
        // Mock return empty list
        _chromaClient.Setup(x => x.List_collectionsAsync(It.IsAny<string>(), It.IsAny<string>(), null, null)).ReturnsAsync(new List<Collection>());
        // Mock create collection exception
        ApiException ex = new ApiException("Mock message", 404, "Mock response", null, null);
        _chromaClient.Setup(x => x.Create_collectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CreateCollectionPayload>())).Throws(ex);

        // Act
        string collectionId = await _service.TryCreateCollection(userId);

        // Assert
        Assert.That(collectionId, Is.EqualTo(null));
    }

    [Test()]
    public async Task TryCreateCollection_TestListException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        // Mock exception
        _chromaClient.Setup(x => x.List_collectionsAsync(It.IsAny<string>(), It.IsAny<string>(), null, null)).Throws(new Exception("Mock exception"));

        // Act
        string collectionId = await _service.TryCreateCollection(userId);

        // Assert
        Assert.That(collectionId, Is.EqualTo(null));
    }

    [Test()]
    public async Task UpsertAsync_TestOk()
    {
        // Arrange
        string collectionId = "123";
        List<ChromaChunk> chunks = new List<ChromaChunk>
        {
            new ChromaChunk(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "doc", 0, "text", new float[1024])
        };
        // Mock return chunk
        _chromaClient.Setup(x => x.Collection_upsertAsync(It.IsAny<string>(), It.IsAny<string>(), collectionId, It.IsAny<UpsertCollectionRecordsPayload>())).Verifiable();

        // Act
        bool result = await _service.UpsertAsync(collectionId, chunks);

        // Assert
        Assert.That(result, Is.EqualTo(true));
    }

    [Test()]
    public async Task UpsertAsync_TestException()
    {
        // Arrange
        string collectionId = "123";
        List<ChromaChunk> chunks = new List<ChromaChunk>
        {
            new ChromaChunk(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "doc", 0, "text", new float[1024])
        };
        // Mock exception
        _chromaClient.Setup(x => x.Collection_upsertAsync(It.IsAny<string>(), It.IsAny<string>(), collectionId, It.IsAny<UpsertCollectionRecordsPayload>())).Throws(new Exception("Mock exception"));

        // Act
        bool result = await _service.UpsertAsync(collectionId, chunks);

        // Assert
        Assert.That(result, Is.EqualTo(false));
    }

    [Test()]
    public async Task UpsertAsync_TestNoId()
    {
        // Arrange
        string collectionId = null;
        List<ChromaChunk> chunks = new List<ChromaChunk>
        {
            new ChromaChunk(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "doc", 0, "text", new float[1024])
        };

        // Act
        bool result = await _service.UpsertAsync(collectionId, chunks);

        // Assert
        Assert.That(result, Is.EqualTo(false));
    }

    [Test()]
    public async Task SearchAsync_TestOk()
    {
        // Arrange
        string collectionId = "123";
        // Mock return chunk
        Dictionary<string, object> metadata = new Dictionary<string, object>
        {
            { "documentName", "aaa" },
            { "chunkIndex", 1 }
        };
        QueryResponse fakeQ = new QueryResponse
        {
            Distances = new List<ICollection<float?>> { new List<float?> { 0 } },  // this is null by default!
            Ids = { new List<string> { "aa" } },  // this NOT null
            Documents = new List<ICollection<string>> { new List<string> { "AA" } },  // this is null by default!
            Metadatas = new List<ICollection<Dictionary<string, object>>> { new List<Dictionary<string, object>> { metadata } },  // this is null by default!
            Embeddings = new List<ICollection<ICollection<float>>> { },
            Uris = null,
            AdditionalProperties = null
        };
        _chromaClient.Setup(x => x.Collection_queryAsync(It.IsAny<string>(), It.IsAny<string>(), collectionId, 5, null, It.IsAny<QueryRequestPayload>())).ReturnsAsync(fakeQ);

        // Act
        IList<ChromaRagChunkResult> result = await _service.SearchAsync(collectionId, Guid.Empty, new float[1024], 5, null);

        // Assert
        Assert.That(result.Count, Is.GreaterThan(0));
        string query = result[0].ToString();
        Assert.That(query.Length, Is.GreaterThan(0));
    }

    [Test()]
    public async Task SearchAsync_TestException()
    {
        // Arrange
        string collectionId = "123";
        // Mock exception
        _chromaClient.Setup(x => x.Collection_queryAsync(It.IsAny<string>(), It.IsAny<string>(), collectionId, 5, null, It.IsAny<QueryRequestPayload>())).Throws(new Exception("Mock exception"));

        // Act
        IList<ChromaRagChunkResult> result = await _service.SearchAsync(collectionId, Guid.Empty, new float[1024], 5, null);

        // Assert
        Assert.That(result, Is.EqualTo(null));
    }

    [Test()]
    public async Task SearchAsync_TestEmptyId()
    {
        // Arrange
        string collectionId = "";

        // Act
        IList<ChromaRagChunkResult> result = await _service.SearchAsync(collectionId, Guid.Empty, new float[1024], 5, null);

        // Assert
        Assert.That(result, Is.EqualTo(null));
    }

    [Test()]
    public async Task DeleteAsync_TestOk()
    {
        // Arrange
        string collectionId = "123";
        DeleteCollectionRecordsResponse deleteResponse = new DeleteCollectionRecordsResponse
        {
            AdditionalProperties = null
        };
        _chromaClient.Setup(x => x.Collection_deleteAsync(It.IsAny<string>(), It.IsAny<string>(), collectionId, It.IsAny<DeleteCollectionRecordsPayload>())).ReturnsAsync(deleteResponse);

        // Act
        bool result = await _service.DeleteAsync(collectionId, Guid.NewGuid());

        // Assert
        Assert.That(result, Is.EqualTo(true));
    }

    [Test()]
    public async Task DeleteAsync_TestException()
    {
        // Arrange
        string collectionId = "123";
        _chromaClient.Setup(x => x.Collection_deleteAsync(It.IsAny<string>(), It.IsAny<string>(), collectionId, It.IsAny<DeleteCollectionRecordsPayload>())).Throws(new Exception("Mock exception"));

        // Act
        bool result = await _service.DeleteAsync(collectionId, Guid.NewGuid());

        // Assert
        Assert.That(result, Is.EqualTo(false));
    }

    [Test()]
    public async Task DeleteAsync_TestEmptyId()
    {
        // Arrange
        string collectionId = "";

        // Act
        bool result = await _service.DeleteAsync(collectionId, Guid.NewGuid());

        // Assert
        Assert.That(result, Is.EqualTo(false));
    }
}