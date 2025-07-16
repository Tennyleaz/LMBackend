using System.Text;
using UglyToad.PdfPig;
using Xceed.Words.NET;

namespace LMBackend;

public static class DocumentSplitter
{
    public static List<string> GetLines(string name, byte[] data)
    {
        // Get extension from name
        FileInfo file = new FileInfo(name);
        switch (file.Extension)
        {
            case ".pdf":
                return GetLinesFromPdf(data);                
            case ".txt":
                return GetLinesFromTxt(data);
            case ".docx":
                return GetLinesFromDocx(data);
            default:
                throw new NotSupportedException("Unsupported type: " + file.Extension);                
        }
    }

    private static List<string> GetLinesFromTxt(byte[] data)
    {
        string text = Encoding.UTF8.GetString(data);
        text = text.Trim();
        return text.Split().ToList();
    }

    private static List<string> GetLinesFromPdf(byte[] data)
    {
        try
        {
            using PdfDocument pdf = PdfDocument.Open(data);
            return pdf.GetPages().Select(p => p.Text).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get text from PDF: " + ex);
            return null;
        }
    }

    private static List<string> GetLinesFromDocx(byte[] data)
    {
        try
        {
            using MemoryStream ms = new MemoryStream(data);
            using DocX docx = DocX.Load(ms);
            return docx.Paragraphs.Select(x => x.Text).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get text from DOCX: " + ex);
            return null;
        }
    }

    public static List<string> SplitText(List<string> units, int maxChunkWords = 250, int overlapWords = 50)
    {
        var chunks = new List<string>();
        int start = 0;

        while (start < units.Count)
        {
            int wordCount = 0;
            int end = start;
            var currentChunk = new List<string>();

            while (end < units.Count && wordCount + units[end].Split(' ').Length <= maxChunkWords)
            {
                currentChunk.Add(units[end]);
                wordCount += units[end].Split(' ').Length;
                end++;
            }

            chunks.Add(string.Join(" ", currentChunk));
            // move start forward for overlap
            start = end - overlapWords < start + 1 ? start + 1 : end - overlapWords;
        }
        return chunks;
    }
}
