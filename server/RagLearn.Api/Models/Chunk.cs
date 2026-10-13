namespace RagLearn.Api.Models;

public class Chunk
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document? Document { get; set; }
    public int Position { get; set; }
    public string? Heading { get; set; }
    public required string Content { get; set; }
    public int CharCount { get; set; }
    public byte[]? Embedding { get; set; }
    public string? EmbeddingModel { get; set; }
}
