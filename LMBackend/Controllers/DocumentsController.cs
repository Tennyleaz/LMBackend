using LMBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentsController : Controller
{
    private readonly ChatContext _context;
    private readonly ChromaVectorStoreService _chromaService;

    public DocumentsController(ChatContext context)
    {
        _context = context;
        _chromaService = new ChromaVectorStoreService();
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IList<Document>>> GetDocuments()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        return await _context.Documents.Where(x => x.UserId == userId).OrderBy(x => x.Name).ToListAsync();
    }

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

        // Get document data
        Document newDoc = Document.FromDto(documentDto, userId);
        List<string> lines = DocumentSplitter.GetLines(newDoc.Name, documentDto.Data);

        // Split documents into chunks
        List<string> documentChunks = DocumentSplitter.SplitText(lines);

        // Call embedding API for each chunk
        await LlmClient.TryCreateLlmInstance();
        List<ChromaChunk> chromaChunks = new List<ChromaChunk>();
        for (int i=0; i< documentChunks.Count; i++)
        {
            float[] embedding = await LlmClient.Instance.GetEmbedding(documentChunks[i]);
            if (embedding == null || embedding.Length == 0)
            {
                continue;
            }
            chromaChunks.Add(new ChromaChunk(userId, newDoc.ChatId, newDoc.Id, newDoc.Name, i, documentChunks[i], embedding));
        }

        // Save embedding to ChromaDB
        await _chromaService.TryCreateCollection(userId);
        bool success = await _chromaService.UpsertAsync(userId, chromaChunks);

        // Save document to SQL
        _context.Documents.Add(newDoc);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDocument), new { id = newDoc.Id }, newDoc);
    }
}
