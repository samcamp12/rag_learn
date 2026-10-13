using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RagLearn.Api.Data;
using RagLearn.Api.Options;
using RagLearn.Api.Services.OpenAi;

namespace RagLearn.Api.Services.Retrieval;

public class EmbeddingIndexer(AppDbContext db, IOpenAiClient openAi, IOptions<OpenAiOptions> options)
{
    private const int BatchSize = 64;

    public async Task<int> EmbedMissingAsync(CancellationToken ct)
    {
        var model = options.Value.EmbeddingModel;
        var pending = await db.Chunks
            .Where(c => c.Embedding == null || c.EmbeddingModel != model)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        for (var start = 0; start < pending.Count; start += BatchSize)
        {
            var batch = pending.Skip(start).Take(BatchSize).ToList();
            var vectors = await openAi.EmbedAsync(batch.Select(c => c.Content).ToList(), ct);

            for (var i = 0; i < batch.Count; i++)
            {
                batch[i].Embedding = VectorMath.ToBytes(vectors[i]);
                batch[i].EmbeddingModel = model;
            }

            await db.SaveChangesAsync(ct);
        }

        return pending.Count;
    }
}
