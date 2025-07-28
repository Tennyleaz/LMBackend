namespace LMBackend.STT;

public class SttResult
{
    public double start { get; set; }
    public double end { get; set; }
    public string text { get; set; }
    public bool isStopped { get; set; }
    public string language { get; set; }
}
