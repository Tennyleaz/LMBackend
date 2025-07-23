using LMBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using LMBackend.RAG;

namespace LMBackend.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class DocumentsController : Controller
{
    private readonly ChatContext _context;
    private readonly ILlmService _llmClient;
    private readonly IVectorStoreService _vectorStore;

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
    [Authorize]
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
    /// Create a document and generate embeddings.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Document>> PostDocument(DocumentDto documentDto)
    {
        // Get userId from JWT claims
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        // Check if it's a valid user
        User user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("No user in database: " + userId);
        }

        // Get document data
        Document newDoc = Document.FromDto(documentDto, userId);
        List<string> lines = DocumentSplitter.GetLines(newDoc.Name, documentDto.Data);

        // Split documents into chunks
        List<string> documentChunks = DocumentSplitter.SplitTextByWords(lines);

        // Call embedding API for each chunk
        List<ChromaChunk> chromaChunks = new List<ChromaChunk>();
        for (int i=0; i< documentChunks.Count; i++)
        {
            float[] embedding = await _llmClient.GetEmbedding(documentChunks[i]);
            if (embedding == null || embedding.Length == 0)
            {
                continue;
            }
            chromaChunks.Add(new ChromaChunk(userId, newDoc.ChatId, newDoc.Id, newDoc.Name, i, documentChunks[i], embedding));
        }

        // Create database and collection if not exist
        //await _chromaService.TryCreateDatabaseForUser();
        string collectionId = await _vectorStore.TryCreateCollection(userId);
        // Save embedding to ChromaDB
        bool success = await _vectorStore.UpsertAsync(collectionId, chromaChunks);
        if (!success)
        {
            return StatusCode(500, "Failed to upsert chromadb");
        }

        // Save document to SQL
        _context.Documents.Add(newDoc);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDocument), new { id = newDoc.Id }, newDoc);
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
}
