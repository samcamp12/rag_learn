using RagLearn.Api.Services;
using Xunit;

namespace RagLearn.Api.Tests;

public class MarkdownChunkerTests
{
    [Fact]
    public void SplitsOnHeadings()
    {
        const string markdown = "# Alpha\n\nFirst paragraph.\n\n## Beta\n\nSecond paragraph.";

        var chunks = MarkdownChunker.Chunk(markdown, chunkSize: 800, overlap: 0);

        Assert.Equal(2, chunks.Count);
        Assert.Equal("Alpha", chunks[0].Heading);
        Assert.Equal("First paragraph.", chunks[0].Content);
        Assert.Equal("Beta", chunks[1].Heading);
        Assert.Equal("Second paragraph.", chunks[1].Content);
    }

    [Fact]
    public void PacksParagraphsUpToChunkSize()
    {
        var paragraph = new string('x', 100);
        var markdown = string.Join("\n\n", paragraph, paragraph, paragraph);

        var chunks = MarkdownChunker.Chunk(markdown, chunkSize: 250, overlap: 0);

        Assert.Equal(2, chunks.Count);
        Assert.All(chunks, c => Assert.True(c.Content.Length <= 250));
    }

    [Fact]
    public void HardSplitsParagraphLongerThanChunkSize()
    {
        var markdown = new string('a', 1000);

        var chunks = MarkdownChunker.Chunk(markdown, chunkSize: 300, overlap: 50);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.Content.Length <= 300));
    }

    [Fact]
    public void RejectsOverlapNotSmallerThanChunkSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MarkdownChunker.Chunk("some text", chunkSize: 100, overlap: 100));
    }
}
