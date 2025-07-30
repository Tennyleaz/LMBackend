using NUnit.Framework;
using LMBackend.RAG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

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
    public async Task TryCreateDatabaseForUser_Test()
    {
        // Arrange
        Database db = new Database
        {
            Name = "db",
            Id = Guid.NewGuid(),
            Tenant = "",
            AdditionalProperties = null
        };
        _chromaClient.Setup(x => x.Get_databaseAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(db);

        // Act
        string name = await _service.TryCreateDatabaseForUser();

        // Assert
        Assert.That(name, Is.EqualTo(db.Name));
    }

    [Test()]
    public void TryCreateCollection_Test()
    {

    }

    [Test()]
    public void UpsertAsync_Test()
    {

    }

    [Test()]
    public void SearchAsync_Test()
    {

    }

    [Test()]
    public void DeleteAsync_Test()
    {

    }
}