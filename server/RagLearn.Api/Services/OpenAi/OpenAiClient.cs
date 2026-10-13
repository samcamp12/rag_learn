using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RagLearn.Api.Options;

namespace RagLearn.Api.Services.OpenAi;

public sealed class OpenAiClient(HttpClient http, IOptions<OpenAiOptions> options) : IOpenAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly OpenAiOptions _settings = options.Value;

    public async Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken ct)
    {
        if (inputs.Count == 0)
        {
            return [];
        }

        var body = new { model = _settings.EmbeddingModel, input = inputs };
        using var response = await PostJsonAsync("v1/embeddings", body, ct);
        var payload = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty embeddings response from OpenAI.");

        return payload.Data.OrderBy(d => d.Index).Select(d => d.Embedding).ToList();
    }

    public async Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, bool jsonMode, CancellationToken ct)
    {
        var body = new
        {
            model = _settings.ChatModel,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = 0,
            response_format = jsonMode ? new { type = "json_object" } : null,
        };

        using var response = await PostJsonAsync("v1/chat/completions", body, ct);
        var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Empty chat response from OpenAI.");

        return payload.Choices.FirstOrDefault()?.Message.Content ?? "";
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var body = new
        {
            model = _settings.ChatModel,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = 0,
            stream = true,
        };

        using var request = CreateRequest(HttpMethod.Post, "v1/chat/completions", body);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessAsync(response, ct);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (await reader.ReadLineAsync(ct) is { } line)
        {
            if (!line.StartsWith("data: ", StringComparison.Ordinal))
            {
                continue;
            }

            var data = line["data: ".Length..];
            if (data == "[DONE]")
            {
                yield break;
            }

            using var doc = JsonDocument.Parse(data);
            if (doc.RootElement.GetProperty("choices")[0].TryGetProperty("delta", out var delta)
                && delta.TryGetProperty("content", out var content))
            {
                var text = content.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string path, object body, CancellationToken ct)
    {
        var response = await http.SendAsync(CreateRequest(HttpMethod.Post, path, body), ct);
        await EnsureSuccessAsync(response, ct);
        return response;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object body)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Run 'dotnet user-secrets set \"OpenAI:ApiKey\" \"...\"' in server/RagLearn.Api.");
        }

        var request = new HttpRequestMessage(method, new Uri(new Uri("https://api.openai.com/"), path))
        {
            Content = JsonContent.Create(body, options: JsonOptions),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException($"OpenAI request failed ({(int)response.StatusCode}): {error}");
    }

    internal sealed record EmbeddingResponse(List<EmbeddingItem> Data);

    internal sealed record EmbeddingItem(int Index, float[] Embedding);

    internal sealed record ChatResponse(List<ChatChoice> Choices);

    internal sealed record ChatChoice(ChatChoiceMessage Message);

    internal sealed record ChatChoiceMessage(string Content);
}
