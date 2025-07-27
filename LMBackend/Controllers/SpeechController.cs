using Asp.Versioning;
using LMBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace LMBackend.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class SpeechController : Controller
{
    private readonly ITtsService _ttsService;

    public SpeechController(ITtsService ttsService)
    {
        _ttsService = ttsService;
    }

    /// <summary>
    /// Get the 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost()]
    public async Task<IActionResult> SpeechToText(SpeechRequest request)
    {
        Guid id = await _ttsService.TextToSpeech(request.Text, request.Locale);

        FileInfo file = _ttsService.GetAudioFile(id);
        if (file == null || !file.Exists)
            return NotFound();

        var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
        HttpContext.Response.OnCompleted(async () =>
        {
            _ttsService.DeleteAudioFile(id);
        });
        return File(stream, "audio/wav");
    }
}
