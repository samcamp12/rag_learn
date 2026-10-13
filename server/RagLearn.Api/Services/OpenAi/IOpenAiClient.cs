namespace RagLearn.Api.Services.OpenAi;

public record ChatMessage(string Role, string Content);

public interface IOpenAiClient
{
    Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken ct);

    Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, bool jsonMode, CancellationToken ct);

    IAsyncEnumerable<string> StreamCompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct);
}
