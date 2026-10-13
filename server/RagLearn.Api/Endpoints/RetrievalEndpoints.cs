using RagLearn.Api.Services.Retrieval;

namespace RagLearn.Api.Endpoints;

public static class RetrievalEndpoints
{
    public static void MapRetrievalEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapPost("/embed", async (EmbeddingIndexer indexer, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(new { embedded = await indexer.EmbedMissingAsync(ct) });
            }
            catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException)
            {
                return OpenAiFailure(ex);
            }
        });

        api.MapPost("/retrieve", async (RetrieveRequest request, RetrievalService retrieval, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Results.BadRequest(new { error = "Enter a query." });
            }

            try
            {
                return Results.Ok(await retrieval.RetrieveAsync(request.Query, ToOptions(request), ct));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or HttpRequestException)
            {
                return OpenAiFailure(ex);
            }
        });

        api.MapPost("/ask", async (RetrieveRequest request, AnswerService answers, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Results.BadRequest(new { error = "Enter a question." });
            }

            try
            {
                return Results.Ok(await answers.AnswerAsync(request.Query, ToOptions(request), ct));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or HttpRequestException)
            {
                return OpenAiFailure(ex);
            }
        });
    }

    private static RetrievalOptions ToOptions(RetrieveRequest request) => new(
        TopK: Math.Clamp(request.TopK ?? 5, 1, 20),
        UseVector: request.UseVector,
        UseBm25: request.UseBm25,
        Filter: new RetrievalFilter(request.FileName, request.IngestedFromUtc, request.IngestedToUtc, request.SectionContains));

    private static IResult OpenAiFailure(Exception ex) => ex is HttpRequestException
        ? Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway)
        : Results.BadRequest(new { error = ex.Message });
}

public record RetrieveRequest(
    string Query,
    int? TopK = null,
    bool UseVector = true,
    bool UseBm25 = true,
    string? FileName = null,
    DateTime? IngestedFromUtc = null,
    DateTime? IngestedToUtc = null,
    string? SectionContains = null);
