using System.Text;

namespace RagLearn.Api.Services;

public record TextChunk(string? Heading, string Content);

public static class MarkdownChunker
{
    public static IReadOnlyList<TextChunk> Chunk(string markdown, int chunkSize, int overlap)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(chunkSize, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(overlap, chunkSize);

        var result = new List<TextChunk>();
        foreach (var (heading, body) in SplitByHeadings(markdown))
        {
            foreach (var piece in PackParagraphs(body, chunkSize, overlap))
            {
                result.Add(new TextChunk(heading, piece));
            }
        }
        return result;
    }

    private static IEnumerable<(string? Heading, string Body)> SplitByHeadings(string markdown)
    {
        string? currentHeading = null;
        var body = new StringBuilder();

        foreach (var line in markdown.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed.StartsWith('#') && trimmed.TrimStart('#').StartsWith(' '))
            {
                if (body.ToString().Trim().Length > 0)
                {
                    yield return (currentHeading, body.ToString());
                }
                body.Clear();
                currentHeading = trimmed.TrimStart('#').Trim();
            }
            else
            {
                body.AppendLine(trimmed);
            }
        }

        if (body.ToString().Trim().Length > 0)
        {
            yield return (currentHeading, body.ToString());
        }
    }

    private static IEnumerable<string> PackParagraphs(string body, int chunkSize, int overlap)
    {
        var paragraphs = body
            .Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0);

        var current = "";
        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > chunkSize)
            {
                if (current.Length > 0)
                {
                    yield return current;
                    current = "";
                }
                foreach (var slice in HardSplit(paragraph, chunkSize, overlap))
                {
                    yield return slice;
                }
                continue;
            }

            if (current.Length == 0)
            {
                current = paragraph;
            }
            else if (current.Length + 2 + paragraph.Length <= chunkSize)
            {
                current = current + "\n\n" + paragraph;
            }
            else
            {
                yield return current;
                var tail = OverlapTail(current, overlap);
                current = tail.Length + 2 + paragraph.Length <= chunkSize
                    ? tail + "\n\n" + paragraph
                    : paragraph;
            }
        }

        if (current.Length > 0)
        {
            yield return current;
        }
    }

    private static IEnumerable<string> HardSplit(string text, int chunkSize, int overlap)
    {
        var step = chunkSize - overlap;
        for (var start = 0; start < text.Length; start += step)
        {
            yield return text.Substring(start, Math.Min(chunkSize, text.Length - start));
            if (start + chunkSize >= text.Length)
            {
                yield break;
            }
        }
    }

    private static string OverlapTail(string text, int overlap)
    {
        if (overlap <= 0 || text.Length <= overlap)
        {
            return "";
        }

        var tail = text[^overlap..];
        var firstSpace = tail.IndexOf(' ');
        return firstSpace >= 0 ? tail[(firstSpace + 1)..] : "";
    }
}
