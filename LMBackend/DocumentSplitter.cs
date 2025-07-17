using System.IO;
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
            case ".htm":
            case ".html":
            case ".js":
            case ".cs":
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

    public static List<string> SplitText(List<string> lines, int maxChunkWords = 250, int overlapWords = 50)
    {
        List<string> chunks = new List<string>();

        // check for empty lines to split into paragraphs
        int start = 0;
        int paragraphCount = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                if (i > start) // Avoid empty paragraphs from consecutive blank lines
                {
                    string paragraph = string.Join("\n", lines.Skip(start).Take(i - start));
                    paragraph = paragraph.Trim();
                    if (!string.IsNullOrWhiteSpace(paragraph))
                    {
                        // add filename and page info to data
                        List<string> results = SplitParagraph(paragraph);
                        for (int j = 0; j < results.Count; j++)
                        {
                            paragraphCount++;
                            string newParagraph = results[j];
                            chunks.Add(newParagraph);
                        }
                    }
                }
                start = i + 1;
            }
        }
        // Handle last paragraph (if the file doesn't end with an empty line)
        if (start < lines.Count)
        {
            string lastParagraph = string.Join("\n", lines.Skip(start).Take(lines.Count - start));
            lastParagraph = lastParagraph.Trim();
            if (!string.IsNullOrWhiteSpace(lastParagraph))
            {
                List<string> results = SplitParagraph(lastParagraph);
                for (int j = 0; j < results.Count; j++)
                {
                    paragraphCount++;
                    string newParagraph = results[j];
                    chunks.Add(newParagraph);
                }
            }
        }

        return chunks;
    }

    /// <summary>
    /// split the input by \n if longer than MAX_TOKEN
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static List<string> SplitParagraph(string input)
    {
        List<string> result = new List<string>();

        // split the input by \n if longer than MAX_TOKEN
        const int MAX_TOKEN = 500;
        if (input.Length >= MAX_TOKEN)
        {
            string[] paragraphs = input.Split(new[] { "\n", "。", ". " }, StringSplitOptions.RemoveEmptyEntries);
            string temp = string.Empty;
            foreach (string paragraph in paragraphs)
            {
                if (paragraph.Length > MAX_TOKEN)
                {
                    // Split the paragraph into smaller chunks
                    for (int i = 0; i < paragraph.Length; i += MAX_TOKEN)
                    {
                        string chunk = paragraph.Substring(i, Math.Min(MAX_TOKEN, paragraph.Length - i));
                        result.Add(chunk);
                    }
                }
                else
                {
                    // add each paragraph up to max length
                    if (temp.Length + paragraph.Length > MAX_TOKEN)
                    {
                        result.Add(temp);
                        temp = string.Empty;
                    }
                    else
                    {
                        temp += paragraph + ". ";
                    }
                }
            }
        }
        else
        {
            result.Add(input);
        }

        return result;
    }

    public static List<string> SplitTextByWords(List<string> lines, int maxChunkWords = 250, int overlapWords = 50)
    {
        // Join all lines into a single document
        var allText = string.Join(" ", lines);
        var words = allText.Split(new[] { ' ', '\n', '\r', '\t', '。', '，', '、', '！', '？', '：', '；' }, StringSplitOptions.RemoveEmptyEntries);

        var chunks = new List<string>();

        int start = 0;
        while (start < words.Length)
        {
            // Calculate end index, not exceeding bounds
            int end = Math.Min(start + maxChunkWords, words.Length);

            // Create chunk
            var chunkWords = words.Skip(start).Take(end - start).ToArray();
            var chunk = string.Join(" ", chunkWords).Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            if (end == words.Length) // last chunk
                break;

            // For overlap, move start forward
            start += (maxChunkWords - overlapWords);
            if (start < 0) start = 0; // safety
        }

        return chunks;
    }
}
