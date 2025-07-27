namespace LMBackend;

public interface ITtsService
{
    /// <summary>
    /// Do TTS, return the audio file id on success.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="locale"></param>
    /// <returns></returns>
    Task<Guid> TextToSpeech(string text, string locale);

    /// <summary>
    /// Returns the audio file by id.
    /// </summary>
    /// <param name="audioId"></param>
    /// <returns></returns>
    FileInfo GetAudioFile(Guid audioId);

    /// <summary>
    /// Deletes the audio file by id.
    /// </summary>
    /// <param name="audioId"></param>
    /// <returns></returns>
    void DeleteAudioFile(Guid audioId);
}
