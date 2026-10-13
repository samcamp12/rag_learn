namespace RagLearn.Api.Services.Retrieval;

// BM25: a keyword ranking formula. Frequent terms in a chunk raise its score,
// rare terms across the corpus raise it more, and long chunks are normalized.
public static class Bm25Ranker
{
    private const double K1 = 1.5;
    private const double B = 0.75;

    public static IReadOnlyList<int> Rank(
        IReadOnlyList<(int Id, IReadOnlyList<string> Tokens)> documents,
        IReadOnlyList<string> queryTokens)
    {
        if (documents.Count == 0 || queryTokens.Count == 0)
        {
            return [];
        }

        var distinctTerms = queryTokens.Distinct().ToList();
        var averageLength = documents.Average(d => (double)d.Tokens.Count);
        if (averageLength == 0)
        {
            return [];
        }

        var documentFrequency = distinctTerms.ToDictionary(
            term => term,
            term => documents.Count(d => d.Tokens.Contains(term)));

        var scored = new List<(int Id, double Score)>();
        foreach (var (id, tokens) in documents)
        {
            var score = 0.0;
            foreach (var term in distinctTerms)
            {
                var df = documentFrequency[term];
                if (df == 0)
                {
                    continue;
                }

                var idf = Math.Log(1 + (documents.Count - df + 0.5) / (df + 0.5));
                var tf = tokens.Count(t => t == term);
                if (tf == 0)
                {
                    continue;
                }

                var lengthNorm = K1 * (1 - B + B * tokens.Count / averageLength);
                score += idf * tf * (K1 + 1) / (tf + lengthNorm);
            }

            if (score > 0)
            {
                scored.Add((id, score));
            }
        }

        return scored
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Id)
            .Select(s => s.Id)
            .ToList();
    }
}
