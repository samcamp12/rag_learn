using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RagLearn.Api.Data;
using RagLearn.Api.Models;
using RagLearn.Api.Options;

namespace RagLearn.Api.Services;

public record IngestResult(int DocumentsProcessed, int ChunksCreated);

public class DocumentIngestionService(AppDbContext db, IOptions<RagOptions> options)
{
    private readonly RagOptions _rag = options.Value;

    public async Task<IngestResult> IngestAllAsync(CancellationToken ct)
    {
        var files = Directory.GetFiles(_rag.DocsPath, "*.md").Order().ToArray();
        var chunksCreated = 0;

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var content = await File.ReadAllTextAsync(file, ct);

            var document = await db.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.FileName == fileName, ct);

            if (document is null)
            {
                document = new Document
                {
                    FileName = fileName,
                    Title = ExtractTitle(content, fileName),
                    Content = content,
                    IngestedAtUtc = DateTime.UtcNow
                };
                db.Documents.Add(document);
            }
            else
            {
                document.Title = ExtractTitle(content, fileName);
                document.Content = content;
                document.IngestedAtUtc = DateTime.UtcNow;
                document.Chunks.Clear();
            }

            var pieces = MarkdownChunker.Chunk(content, _rag.ChunkSize, _rag.ChunkOverlap);
            for (var i = 0; i < pieces.Count; i++)
            {
                document.Chunks.Add(new Chunk
                {
                    Position = i,
                    Heading = pieces[i].Heading,
                    Content = pieces[i].Content,
                    CharCount = pieces[i].Content.Length
                });
            }
            chunksCreated += pieces.Count;
        }

        await db.SaveChangesAsync(ct);
        return new IngestResult(files.Length, chunksCreated);
    }

    private static string ExtractTitle(string content, string fileName)
    {
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("# "))
            {
                return trimmed[2..].Trim();
            }
        }
        return Path.GetFileNameWithoutExtension(fileName);
    }
}
