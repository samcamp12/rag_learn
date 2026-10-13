using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RagLearn.Api.Data;
using RagLearn.Api.Options;
using RagLearn.Api.Services.OpenAi;

namespace RagLearn.Api.Services.Retrieval;

public class RetrievalService(AppDbContext db, IOpenAiClient openAi, IOptions<OpenAiOptions> options)
{
    public async Task<RetrievalResult> RetrieveAsync(string query, RetrievalOptions retrieval, CancellationToken ct)
    {
        if (!retrieval.UseVector && !retrieval.UseBm25)
        {
            throw new ArgumentException("Enable vector search, BM25, or both.");
        }

        var model = options.Value.EmbeddingModel;
        var stages = new List<string>();

        var candidates = await LoadCandidatesAsync(retrieval.Filter, ct);
        var embedded = candidates.Where(c => c.Embedding is not null && c.EmbeddingModel == model).ToList();
        stages.Add($"Filter: {candidates.Count} chunk(s) match before ranking.");

        var pool = Math.Max(retrieval.TopK * 4, 20);
        var rankings = new List<IReadOnlyList<int>>();
        var vectorRanks = new Dictionary<int, (int Rank, double Score)>();
        var bm25Ranks = new Dictionary<int, int>();

        if (retrieval.UseVector)
        {
            if (embedded.Count == 0)
            {
                stages.Add("Vector: skipped, no chunks have embeddings for the current model. Run POST /api/embed.");
            }
            else
            {
                var queryVector = (await openAi.EmbedAsync([query], ct))[0];
                var vectorRanking = embedded
                    .Select(c => (c.ChunkId, Score: VectorMath.CosineSimilarity(queryVector, VectorMath.FromBytes(c.Embedding!))))
                    .OrderByDescending(s => s.Score)
                    .ThenBy(s => s.ChunkId)
                    .Take(pool)
                    .ToList();

                for (var i = 0; i < vectorRanking.Count; i++)
                {
                    vectorRanks[vectorRanking[i].ChunkId] = (i + 1, vectorRanking[i].Score);
                }

                rankings.Add(vectorRanking.Select(s => s.ChunkId).ToList());
                stages.Add($"Vector: cosine similarity over {embedded.Count} embedded chunk(s), keeping top {vectorRanking.Count}.");
            }
        }

        if (retrieval.UseBm25)
        {
            var documents = candidates
                .Select(c => (Id: c.ChunkId, Tokens: (IReadOnlyList<string>)TextTokenizer.Tokenize(c.Content)))
                .ToList();
            var bm25Ranking = Bm25Ranker.Rank(documents, TextTokenizer.Tokenize(query)).Take(pool).ToList();

            for (var i = 0; i < bm25Ranking.Count; i++)
            {
                bm25Ranks[bm25Ranking[i]] = i + 1;
            }

            rankings.Add(bm25Ranking);
            stages.Add($"BM25: {bm25Ranking.Count} chunk(s) contain a query term.");
        }

        var byId = candidates.ToDictionary(c => c.ChunkId);
        var fused = RankFusion.Fuse(rankings).Take(retrieval.TopK).ToList();
        stages.Add($"RRF: fused {rankings.Count} ranking(s) with k={RankFusion.DefaultK}, keeping top {fused.Count}.");

        var chunks = new List<RetrievedChunk>();
        for (var i = 0; i < fused.Count; i++)
        {
            var c = byId[fused[i].Id];
            var hasVector = vectorRanks.TryGetValue(c.ChunkId, out var vector);
            var hasBm25 = bm25Ranks.TryGetValue(c.ChunkId, out var bm25Rank);

            chunks.Add(new RetrievedChunk(
                c.ChunkId,
                c.DocumentId,
                c.FileName,
                c.Title,
                c.Heading,
                c.Content,
                i + 1,
                fused[i].Score,
                hasVector ? vector.Rank : null,
                hasVector ? vector.Score : null,
                hasBm25 ? bm25Rank : null));
        }

        return new RetrievalResult(chunks, candidates.Count, embedded.Count, stages);
    }

    private async Task<List<Candidate>> LoadCandidatesAsync(RetrievalFilter? filter, CancellationToken ct)
    {
        var query = db.Chunks.AsNoTracking().AsQueryable();

        if (filter?.FileName is { Length: > 0 } fileName)
        {
            query = query.Where(c => c.Document!.FileName == fileName);
        }

        if (filter?.IngestedFromUtc is { } from)
        {
            query = query.Where(c => c.Document!.IngestedAtUtc >= from);
        }

        if (filter?.IngestedToUtc is { } to)
        {
            query = query.Where(c => c.Document!.IngestedAtUtc <= to);
        }

        if (filter?.SectionContains is { Length: > 0 } section)
        {
            query = query.Where(c => c.Heading != null && c.Heading.Contains(section));
        }

        return await query
            .OrderBy(c => c.Id)
            .Select(c => new Candidate(
                c.Id,
                c.DocumentId,
                c.Document!.FileName,
                c.Document!.Title,
                c.Heading,
                c.Content,
                c.Embedding,
                c.EmbeddingModel))
            .ToListAsync(ct);
    }

    private sealed record Candidate(
        int ChunkId,
        int DocumentId,
        string FileName,
        string Title,
        string? Heading,
        string Content,
        byte[]? Embedding,
        string? EmbeddingModel);
}
