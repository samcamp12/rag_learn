using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RagLearn.Api.Data;
using RagLearn.Api.Options;
using RagLearn.Api.Services;

namespace RagLearn.Api.Endpoints;

public static class ApiEndpoints
{
    private const int MaxSqlRows = 200;

    public static void MapRagEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));

        api.MapGet("/status", async (AppDbContext db, IOptions<RagOptions> rag, IOptions<OpenAiOptions> openAi, CancellationToken ct) =>
            Results.Ok(new StatusResponse(
                rag.Value.DatabasePath,
                rag.Value.DocsPath,
                await db.Documents.CountAsync(ct),
                await db.Chunks.CountAsync(ct),
                await db.Chunks.CountAsync(c => c.Embedding != null, ct),
                openAi.Value.IsConfigured)));

        api.MapGet("/documents", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Documents
                .OrderBy(d => d.FileName)
                .Select(d => new DocumentSummary(d.Id, d.FileName, d.Title, d.Content.Length, d.Chunks.Count(), d.IngestedAtUtc))
                .ToListAsync(ct)));

        api.MapGet("/documents/{id:int}/chunks", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Documents.AnyAsync(d => d.Id == id, ct))
            {
                return Results.NotFound();
            }

            var chunks = await db.Chunks
                .Where(c => c.DocumentId == id)
                .OrderBy(c => c.Position)
                .Select(c => new ChunkDto(c.Id, c.Position, c.Heading, c.CharCount, c.Content))
                .ToListAsync(ct);

            return Results.Ok(chunks);
        });

        api.MapPost("/ingest", async (DocumentIngestionService ingestion, CancellationToken ct) =>
            Results.Ok(await ingestion.IngestAllAsync(ct)));

        api.MapPost("/sql", async (SqlRequest request, IOptions<RagOptions> rag, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return Results.BadRequest(new { error = "Enter a SQL statement." });
            }

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = rag.Value.DatabasePath,
                Mode = SqliteOpenMode.ReadOnly
            }.ToString();

            try
            {
                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync(ct);

                await using var command = connection.CreateCommand();
                command.CommandText = request.Sql;
                await using var reader = await command.ExecuteReaderAsync(ct);

                var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                var rows = new List<object?[]>();
                while (rows.Count < MaxSqlRows && await reader.ReadAsync(ct))
                {
                    var row = new object?[reader.FieldCount];
                    for (var i = 0; i < row.Length; i++)
                    {
                        row[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    rows.Add(row);
                }

                return Results.Ok(new SqlResponse(columns, rows));
            }
            catch (SqliteException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }
}

public record StatusResponse(
    string DatabasePath,
    string DocsPath,
    int DocumentCount,
    int ChunkCount,
    int EmbeddedChunkCount,
    bool OpenAiConfigured);

public record DocumentSummary(
    int Id,
    string FileName,
    string Title,
    int ContentLength,
    int ChunkCount,
    DateTime IngestedAtUtc);

public record ChunkDto(int Id, int Position, string? Heading, int CharCount, string Content);

public record SqlRequest(string Sql);

public record SqlResponse(List<string> Columns, List<object?[]> Rows);
