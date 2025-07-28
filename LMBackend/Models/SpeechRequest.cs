namespace LMBackend.Models;

public class SpeechRequest
{
    public string Text { get; set; }
    public SpeechLocale Locale { get; set; }
}

public enum SpeechLocale
{
    English = 'a',
    Spanish = 'e',
    French = 'f',
    Hindi = 'h',
    Italian = 'i',
    Japanese = 'j',
    Portuguese = 'p',
    Chinese = 'z'
}
