namespace RagLearn.Api.Options;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = "";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public string ChatModel { get; set; } = "gpt-4o-mini";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
