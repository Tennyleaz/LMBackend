using LMBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using LMBackend.RAG;
using LMBackend.RAG.Chroma;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LMBackend.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class DocumentsController : Controller
{
    private readonly ChatContext _context;
    private readonly ILlmService _llmClient;
    private readonly IVectorStoreService _vectorStore;
    private static readonly Guid MCP_SERVER_ID = new Guid("{343F7B40-EED8-4A8C-84FF-019C2CBA5EE3}");
    private static readonly Guid MCP_REGISTRY_ID = new Guid("{2EF4DAEB-15DB-4A1B-B0DF-A9289B310DF6}");


    public DocumentsController(ChatContext context, ILlmService llmService, IVectorStoreService vectorStore)
    {
        _context = context;
        _llmClient = llmService;
        _vectorStore = vectorStore;
    }

    /// <summary>
    /// Get all documents by me.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        return await _context.Documents.Where(x => x.UserId == userId).OrderBy(x => x.Name).ToListAsync();
    }

    /// <summary>
    /// Get a document by id.
    /// </summary>
    [HttpGet("{id}")]
    //[Authorize]
    public async Task<ActionResult<Document>> GetDocument(Guid id)
    {
        Document item = await _context.Documents.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    /// <summary>
    /// Insert chunks into chromadb. Will not create new document.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("chunks")]
    public async Task<ActionResult<Document>> PostDocumentChunks(DocumentChunkDto request)
    {
        // Debug log
        Console.WriteLine($"PostDocumentChunks request: Name={request.Name}, Chunks count={request.Chunks?.Count}.");
        if (request.Chunks == null || request.Chunks.Count == 0)
        {
            return StatusCode(500, "Chunks are null or empty!");
        }

        // Get userId from JWT claims
        Guid userId = User.GetUserId();

        // Create fake document
        Document newDoc = new Document()
        {
            ChatId = Guid.Empty,
            UserId = userId,
            CreatedTime = DateTime.Now,
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        // Create database and collection if not exist
        string collectionId = await _vectorStore.TryCreateCollection(userId);
        if (collectionId == null)
        {
            return StatusCode(500, "Failed to create chromadb collection");
        }

        // Call embedding API for each chunk
        List<ChromaChunk> chromaChunks;
        try
        {
            IEnumerable<string> chunks = request.Chunks.Select(x => x.ToString());
            chromaChunks = await GetEmbeddingsAndUpsert(userId, collectionId, chunks, newDoc);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        // Save document to SQL
        if (userId != Guid.Empty)
        {
            _context.Documents.Add(newDoc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocument), new { id = newDoc.Id }, newDoc);
        }
        // Fake document if no user id
        return Ok(newDoc);
    }

    /// <summary>
    /// Insert list of MCP servers into chromadb. Will not create new document.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("mcpServerChunk")]
    public async Task<ActionResult> PostMcpServerChunks(List<McpServerChunkDto> request)
    {
        // Debug log
        Console.WriteLine($"PostMcpServerChunks chunks count={request.Count}.");
        if (request.Count == 0)
        {
            return StatusCode(500, "Chunks are null or empty!");
        }

        // Create database and collection if not exist
        string collectionId = await _vectorStore.TryCreateCollection(MCP_SERVER_ID);
        if (collectionId == null)
        {
            return StatusCode(500, "Failed to create chromadb collection");
        }

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        try
        {
            // Call embedding API for each chunk
            List<ChromaChunk> chromaChunks = new List<ChromaChunk>();
            foreach (McpServerChunkDto dto in request)
            {
                // Skip MCP server with no tools
                if (dto.Tools == null || dto.Tools.Count == 0)
                    continue;

                string chunkText = dto.ToString();
                float[] embedding = await _llmClient.GetEmbedding(chunkText);
                if (embedding == null || embedding.Length == 0)
                {
                    throw new Exception("Failed to get embeddings");
                }

                string rawJson = JsonSerializer.Serialize(dto, options);
                ChromaChunk oneChunk = new ChromaChunk(dto, rawJson, embedding);
                chromaChunks.Add(oneChunk);
            }

            // Save embedding to ChromaDB, split into chunks of 200 each
            await BatchUpsertChunks(collectionId, chromaChunks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        return Ok();
    }

    /// <summary>
    /// Insert list of MCP servers from McpRegistry into chromadb. Will not create new document.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("mcpRegistryChunk")]
    public async Task<ActionResult> PostMcpRegistryChunks(List<McpRegistryChunkDto> request)
    {
        // Debug log
        Console.WriteLine($"PostMcpServerChunks chunks count={request.Count}.");
        if (request.Count == 0)
        {
            return StatusCode(500, "Chunks are null or empty!");
        }

        // Create database and collection if not exist
        string collectionId = await _vectorStore.TryCreateCollection(MCP_REGISTRY_ID);
        if (collectionId == null)
        {
            return StatusCode(500, "Failed to create chromadb collection");
        }

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        try
        {
            // Call embedding API for each chunk
            List<ChromaChunk> chromaChunks = new List<ChromaChunk>();
            foreach (McpRegistryChunkDto dto in request)
            {
                // Skip MCP server with no title and description
                if (string.IsNullOrWhiteSpace(dto.Description) && string.IsNullOrWhiteSpace(dto.Title))
                    continue;

                string chunkText = dto.ToString();
                float[] embedding = await _llmClient.GetEmbedding(chunkText);
                if (embedding == null || embedding.Length == 0)
                {
                    throw new Exception("Failed to get embeddings");
                }

                string rawJson = JsonSerializer.Serialize(dto, options);
                ChromaChunk oneChunk = new ChromaChunk(dto, rawJson, embedding);
                chromaChunks.Add(oneChunk);
            }

            // Save embedding to ChromaDB, split into chunks of 200 each
            await BatchUpsertChunks(collectionId, chromaChunks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        return Ok();
    }

    /// <summary>
    /// Create a document and generate embeddings.
    /// </summary>
    [HttpPost]
    //[Authorize]
    public async Task<ActionResult<Document>> PostDocument(DocumentDto documentDto)
    {
        // Get userId from JWT claims
        Guid userId = User.GetUserId();
        //if (userId == Guid.Empty)
        //{
        //    return Unauthorized();
        //}
        //// Check if it's a valid user
        //User user = await _context.Users.FindAsync(userId);
        //if (user == null)
        //{
        //    return NotFound("No user in database: " + userId);
        //}
        //userId = Guid.Empty;

        // Create database and collection if not exist
        //await _chromaService.TryCreateDatabaseForUser();
        string collectionId = await _vectorStore.TryCreateCollection(userId);
        if (collectionId == null)
        {
            return StatusCode(500, "Failed to create chromadb collection");
        }

        // Get document data
        Document newDoc = Document.FromDto(documentDto, userId);
        List<string> lines = DocumentSplitter.GetLines(newDoc.Name, documentDto.Data);

        // Split documents into chunks
        List<string> documentChunks = DocumentSplitter.SplitTextByWords(lines);

        // Call embedding API for each chunk
        try
        {
            List<ChromaChunk> chromaChunks = await GetEmbeddingsAndUpsert(userId, collectionId, documentChunks, newDoc);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        // Save document to SQL
        if (userId != Guid.Empty)
        {
            _context.Documents.Add(newDoc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocument), new { id = newDoc.Id }, newDoc);
        }
        // Fake document if no user id
        return Ok(newDoc);
    }

    private async Task<List<ChromaChunk>> GetEmbeddingsAndUpsert(Guid userId, string collectionId, IEnumerable<string> chunks, Document newDoc)
    {
        // Call embedding API for each chunk
        int index = 0, count = 0;
        List<ChromaChunk> chromaChunks = new List<ChromaChunk>();
        foreach (string chunkText in chunks)
        {
            float[] embedding = await _llmClient.GetEmbedding(chunkText);
            if (embedding == null || embedding.Length == 0)
            {
                throw new Exception("Failed to get embeddings");
            }
            chromaChunks.Add(new ChromaChunk(userId, newDoc.ChatId, newDoc.Id, newDoc.Name, index, chunkText, embedding));
            index++;
        }

        // Save embedding to ChromaDB, split into chunks of 200 each
        await BatchUpsertChunks(collectionId, chromaChunks);

        return chromaChunks;
    }

    private async Task BatchUpsertChunks(string collectionId, List<ChromaChunk> chromaChunks)
    {
        // Save embedding to ChromaDB, split into chunks of 200 each
        int index = 0, count = 0;
        bool success;
        List<ChromaChunk> tempChunk = new List<ChromaChunk>();
        for (index = 0; index < chromaChunks.Count; index++)
        {
            tempChunk.Add(chromaChunks[index]);
            count++;

            if (count >= 200)
            {
                // upsert once
                success = await _vectorStore.UpsertAsync(collectionId, tempChunk);
                if (!success)
                {
                    throw new Exception("Failed to upsert chromadb");
                }
                // clear for next batch
                tempChunk.Clear();
                count = 0;
            }
        }
        // upsert last chunk
        if (tempChunk.Count > 0)
        {
            success = await _vectorStore.UpsertAsync(collectionId, tempChunk);
            if (!success)
            {
                throw new Exception("Failed to upsert chromadb");
            }
        }
    }

    /// <summary>
    /// Delete a document by id.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        Document doc = await _context.Documents.FindAsync(id);
        if (doc == null)
        {
            return NotFound();
        }

        string collectionId = await _vectorStore.TryCreateCollection(userId);
        bool result = await _vectorStore.DeleteAsync(collectionId, id);

        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Directly query chromadb for chunks.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("query")]
    //[Authorize]
    public async Task<ActionResult<List<DocumentSearchResponse>>> QueryAsync(DocumentSearchRequest request)
    {
        // Get userId from JWT claims
        Guid userId = User.GetUserId();
        //if (userId == Guid.Empty)
        //{
        //    return Unauthorized();
        //}
        //// Check if it's a valid user
        //User user = await _context.Users.FindAsync(userId);
        //if (user == null)
        //{
        //    return NotFound("No user in database: " + userId);
        //}

        // Generate embedding for user query
        float[] embedding = await _llmClient.GetEmbedding(request.Query);
        if (embedding == null)
        {
            return BadRequest("Failed to generate embedding from query.");
        }

        string collectionId = await _vectorStore.TryCreateCollection(userId);
        // Save embedding to ChromaDB
        IList<ChromaRagChunkResult> chunkResult = await _vectorStore.SearchAsync(collectionId, Guid.Empty, embedding, request.TopK, null);
        if (chunkResult == null)
        {
            return BadRequest("Failed to search chroma DB");
        }

        List<DocumentSearchResponse> results = new List<DocumentSearchResponse>();
        foreach (var ccr in chunkResult)
        {
            results.Add(new DocumentSearchResponse
            {
                Id = ccr.Id,
                Document = ccr.Document,
                Metadata = ccr.Metadata,
                Distance = ccr.Distance
            });
        }

        return results;
    }

    /// <summary>
    /// Query chromadb for MCP server.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("queryMcpServer")]
    //[Authorize]
    public async Task<ActionResult<List<DocumentSearchResponse>>> QueryMcpServerAsync(DocumentSearchRequest request)
    {
        // Generate embedding for user query
        float[] embedding = await _llmClient.GetEmbedding(request.Query);
        if (embedding == null)
        {
            return BadRequest("Failed to generate embedding from query.");
        }

        string collectionId = await _vectorStore.TryCreateCollection(MCP_SERVER_ID);
        // Save embedding to ChromaDB
        IList<ChromaRagChunkResult> chunkResult = await _vectorStore.SearchAsync(collectionId, Guid.Empty, embedding, request.TopK, null);
        if (chunkResult == null)
        {
            return BadRequest("Failed to search chroma DB");
        }

        List<DocumentSearchResponse> results = new List<DocumentSearchResponse>();
        foreach (var ccr in chunkResult)
        {
            results.Add(new DocumentSearchResponse
            {
                Id = ccr.Id,
                Document = ccr.Document,
                Metadata = ccr.Metadata,
                Distance = ccr.Distance
            });
        }

        return Ok(results);
    }

    /// <summary>
    /// Query chromadb for McpRegistry.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("queryMcpRegistry")]
    public async Task<ActionResult<List<DocumentSearchResponse>>> QueryMcpRegistryAsync(DocumentSearchRequest request)
    {
        // Generate embedding for user query
        float[] embedding = await _llmClient.GetEmbedding(request.Query);
        if (embedding == null)
        {
            return BadRequest("Failed to generate embedding from query.");
        }

        string collectionId = await _vectorStore.TryCreateCollection(MCP_REGISTRY_ID);
        // Save embedding to ChromaDB
        IList<ChromaRagChunkResult> chunkResult = await _vectorStore.SearchAsync(collectionId, Guid.Empty, embedding, request.TopK, null);
        if (chunkResult == null)
        {
            return BadRequest("Failed to search chroma DB");
        }

        List<DocumentSearchResponse> results = new List<DocumentSearchResponse>();
        foreach (var ccr in chunkResult)
        {
            results.Add(new DocumentSearchResponse
            {
                Id = ccr.Id,
                Document = ccr.Document,
                Metadata = ccr.Metadata,
                Distance = ccr.Distance
            });
        }

        return Ok(results);
    }

    /// <summary>
    /// Clear the chroma db collection for McpRegistry chunks.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("mcpRegistry")]
    public async Task<ActionResult> ClearMcpRegistryChunks()
    {
        string collectionId = await _vectorStore.TryCreateCollection(MCP_REGISTRY_ID);
        if (!string.IsNullOrEmpty(collectionId))
        {
            bool result = await _vectorStore.ClearCollectionAsync(collectionId);
            if (result)
            {
                return NoContent();
            }

            return BadRequest("Failed to delete collection id=" + collectionId);
        }

        return NotFound("Cannot find DB for McpRegistry.");
    }
}
