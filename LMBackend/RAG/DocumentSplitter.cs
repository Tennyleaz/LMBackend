using System.IO;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Xceed.Words.NET;

namespace LMBackend.RAG;

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

    private static List<string> GetLinesFromPdf(byte[] byteArray)
    {
        List<string> result = new List<string>();
        try
        {
            using PdfDocument document = PdfDocument.Open(byteArray);
            foreach (Page page in document.GetPages())
            {
                string data = string.Empty;
                List<Line> lines = new List<Line>();
                var currLine = new Line();
                lines.Add(currLine);
                foreach (Word word in page.GetWords())
                {
                    var box = word.BoundingBox;
                    if (!currLine.InSameLine(word))
                    {
                        currLine = new Line();
                        lines.Add(currLine);
                    }
                    currLine.AddWord(word);
                }
                var leftMargin = lines.Min(l => l.Left);
                foreach (var line in lines)
                {
                    var indent = line.Left - leftMargin;
                    if (indent > 0)
                    {
                        //Console.Write(new string(' ', (int)indent / 14));
                        int count = (int)indent / 14;
                        for (int i = 0; i < count; i++)
                            data += ' ';
                    }

                    data += "\n" + line.ToString();
                    //Console.WriteLine(line);
                }
                data = data.Trim();
                if (!string.IsNullOrWhiteSpace(data))
                {
                    result.Add(data);
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get text from PDF: " + ex);
            return null;
        }
    }

    private class Line
    {
        public List<Word> Words { get; set; } = new List<Word>();
        public double? Bottom { get; set; } = null;
        public double Left
        {
            get
            {
                if (Words.Count == 0)
                    return 0;
                return Words.Min(w => w.BoundingBox.Left);
            }
        }

        public void AddWord(Word word)
        {
            if (!InSameLine(word))
                throw new Exception("Word is not in the same line");
            Words.Add(word);
            if (Bottom == null)
            {
                Bottom = word.BoundingBox.Bottom;
            }
            else
            {
                Bottom = Math.Max(Bottom.Value, word.BoundingBox.Bottom);
            }
        }
        public bool InSameLine(Word word)
        {
            return Bottom == null ||
                   Math.Abs(word.BoundingBox.Bottom - Bottom.Value) < word.BoundingBox.Height;
        }

        public string ToString(int leftMargin)
        {
            if (Words.Count == 0)
                return string.Empty;
            var sb = new StringBuilder();
            Word prevWord = null;
            var avgCharWidth = Convert.ToInt32(Words.Average(w => w.BoundingBox.Width / w.Text.Length));
            if (leftMargin > 0) sb.Append(new string(' ', (int)(Words[0].BoundingBox.Left - leftMargin) / avgCharWidth));
            foreach (var word in Words.OrderBy(w => w.BoundingBox.Left))
            {
                if (prevWord != null && word.BoundingBox.Left - prevWord.BoundingBox.Right > avgCharWidth)
                    sb.Append(new string(' ', (int)(word.BoundingBox.Left - prevWord.BoundingBox.Right) / avgCharWidth));
                sb.Append(word.Text + " ");
                prevWord = word;
            }
            return sb.ToString();
        }

        public override string ToString() => ToString(0);
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
            start += maxChunkWords - overlapWords;
            if (start < 0) start = 0; // safety
        }

        return chunks;
    }
}
