namespace RagLearn.Api.Options;

public class RagOptions
{
    public const string SectionName = "Rag";

    public string DocsPath { get; set; } = "";
    public string DatabasePath { get; set; } = "";
    public string EvalPath { get; set; } = "";
    public int ChunkSize { get; set; } = 800;
    public int ChunkOverlap { get; set; } = 100;
}
