using RagLearn.Api.Services.OpenAi;

namespace RagLearn.Api.Services.Retrieval;

public record AnswerResult(string Answer, IReadOnlyList<RetrievedChunk> Sources, IReadOnlyList<string> Stages);

public class AnswerService(RetrievalService search, IOpenAiClient openAi)
{
    private const string SystemPrompt =
        "You answer questions using only the numbered sources provided. " +
        "Cite sources inline with their number, like [1]. " +
        "If the sources do not contain the answer, say you don't know.";

    public async Task<AnswerResult> AnswerAsync(string question, RetrievalOptions options, CancellationToken ct)
    {
        var result = await search.RetrieveAsync(question, options, ct);
        if (result.Chunks.Count == 0)
        {
            return new AnswerResult("I couldn't find any relevant sources in the documents.", result.Chunks, result.Stages);
        }

        var sources = string.Join("\n\n", result.Chunks.Select((c, i) =>
            $"[{i + 1}] {c.FileName} > {c.Heading ?? c.Title}\n{c.Content}"));

        var answer = await openAi.CompleteAsync(
        [
            new ChatMessage("system", SystemPrompt),
            new ChatMessage("user", $"Sources:\n\n{sources}\n\nQuestion: {question}"),
        ], false, ct);

        return new AnswerResult(answer, result.Chunks, result.Stages);
    }
}
