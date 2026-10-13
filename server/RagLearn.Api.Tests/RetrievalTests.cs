using RagLearn.Api.Services.Retrieval;
using Xunit;

namespace RagLearn.Api.Tests;

public class TextRetrievalTests
{
    [Fact]
    public void Tokenize_LowercasesAndDropsPunctuationStopWordsAndSingleLetters()
    {
        var tokens = TextTokenizer.Tokenize("What is BM25? It's a x keyword-ranking formula.");

        Assert.Equal(new[] { "bm25", "keyword", "ranking", "formula" }, tokens);
    }

    [Fact]
    public void Bm25_RanksChunkContainingQueryTermsFirst()
    {
        var documents = new List<(int Id, IReadOnlyList<string> Tokens)>
        {
            (1, TextTokenizer.Tokenize("vector database stores embeddings")),
            (2, TextTokenizer.Tokenize("keyword search uses bm25 scoring for keyword matches")),
            (3, TextTokenizer.Tokenize("chunks are split with overlap")),
        };

        var ranking = Bm25Ranker.Rank(documents, TextTokenizer.Tokenize("bm25 keyword"));

        Assert.Equal(2, ranking[0]);
        Assert.DoesNotContain(3, ranking);
    }

    [Fact]
    public void Bm25_ReturnsEmptyWhenNoTermMatches()
    {
        var documents = new List<(int Id, IReadOnlyList<string> Tokens)>
        {
            (1, TextTokenizer.Tokenize("chunks are split with overlap")),
        };

        Assert.Empty(Bm25Ranker.Rank(documents, TextTokenizer.Tokenize("kubernetes")));
    }
}

public class VectorAndFusionTests
{
    [Fact]
    public void CosineSimilarity_IdenticalVectorsAreOne_OrthogonalAreZero()
    {
        float[] a = [1, 2, 3];
        float[] perpendicular = [2, -1, 0];

        Assert.Equal(1.0, VectorMath.CosineSimilarity(a, a), precision: 6);
        Assert.Equal(0.0, VectorMath.CosineSimilarity(a, perpendicular), precision: 6);
        Assert.Equal(0.0, VectorMath.CosineSimilarity(new float[] { 1, 0 }, new float[] { 0, 0 }), precision: 6);
    }

    [Fact]
    public void EmbeddingBytes_RoundTripPreservesValues()
    {
        float[] original = [0.25f, -1.5f, 3.125f];

        var roundTripped = VectorMath.FromBytes(VectorMath.ToBytes(original));

        Assert.Equal(original, roundTripped);
    }

    [Fact]
    public void RankFusion_ItemRankedWellInBothListsBeatsItemTopOfOnlyOne()
    {
        var fused = RankFusion.Fuse([[1, 2, 3], [3, 1, 4]]);

        Assert.Equal(new[] { 1, 3, 2, 4 }, fused.Select(f => f.Id));
        Assert.Equal(1.0 / 61 + 1.0 / 62, fused[0].Score, precision: 10);
    }
}
