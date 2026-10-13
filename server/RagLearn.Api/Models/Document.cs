namespace RagLearn.Api.Models;

public class Document
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime IngestedAtUtc { get; set; }

    public List<Chunk> Chunks { get; set; } = [];
}
