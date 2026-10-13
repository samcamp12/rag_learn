namespace RagLearn.Api.Services.Retrieval;

// Reciprocal Rank Fusion: each ranking contributes 1 / (k + rank) for every item.
// Items that appear high in several rankings rise to the top, without needing
// the raw scores from each method to be comparable.
public static class RankFusion
{
    public const int DefaultK = 60;

    public static List<(int Id, double Score)> Fuse(IEnumerable<IReadOnlyList<int>> rankings, int k = DefaultK)
    {
        var scores = new Dictionary<int, double>();

        foreach (var ranking in rankings)
        {
            for (var i = 0; i < ranking.Count; i++)
            {
                var id = ranking[i];
                scores[id] = scores.GetValueOrDefault(id) + 1.0 / (k + i + 1);
            }
        }

        return scores
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }
}
