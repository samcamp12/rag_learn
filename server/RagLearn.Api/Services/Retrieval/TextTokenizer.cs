namespace RagLearn.Api.Services.Retrieval;

public static class TextTokenizer
{
    private static readonly HashSet<string> StopWords =
    [
        "the", "a", "an", "and", "or", "of", "to", "in", "is", "it", "for", "on", "with", "as", "by", "be", "this", "that", "are", "was", "what", "how", "does", "do",
    ];

    public static List<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                current.Append(char.ToLowerInvariant(ch));
                continue;
            }

            AddToken(tokens, current);
        }
        AddToken(tokens, current);

        return tokens;
    }

    private static void AddToken(List<string> tokens, System.Text.StringBuilder current)
    {
        if (current.Length >= 2)
        {
            var token = current.ToString();
            if (!StopWords.Contains(token))
            {
                tokens.Add(token);
            }
        }
        current.Clear();
    }
}
