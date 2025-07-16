namespace LMBackend;

public static class DocumentSplitter
{
    public static List<string> GetLines(string name, byte[] data)
    {
        List<string> lines = new List<string>();
        return lines;
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
