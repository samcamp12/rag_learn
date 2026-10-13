# Chunking in this project

`MarkdownChunker` in `server/RagLearn.Api/Services/MarkdownChunker.cs` converts one Markdown file into a list of chunks. It has no database or HTTP code, so you can read it on its own.

## The algorithm

1. **Split by headings.** Each line starting with `#` and a space begins a new section. The heading text is stored with the chunk.
2. **Split by paragraphs.** Within a section, paragraphs are separated by blank lines.
3. **Pack paragraphs.** Add paragraphs to the current chunk until the next one would exceed `ChunkSize`. Then start a new chunk.
4. **Overlap.** If it still fits in `ChunkSize`, the new chunk begins with the last `ChunkOverlap` characters of the previous chunk, cut at a space so no word is split.
5. **Hard split.** A paragraph longer than `ChunkSize` is cut into pieces by size, with the same overlap.

## Reading the code

Start with `Chunk(...)`, then follow the calls to `SplitByHeadings`, `PackParagraphs`, and `OverlapTail`. Each method does one thing. This is the same style you would use for a C# helper class with private static methods.

## Compared with C#

- `IEnumerable<T>` with `yield return` works like a lazy LINQ pipeline. The method produces values one at a time.
- `string[..]` and `text[^n..]` are C# 8 range and index syntax. `text[^overlap..]` means the last `overlap` characters.
- `ArgumentOutOfRangeException.ThrowIfLessThanOrEqual` is a guard clause. It replaces a manual `if ... throw`.

## Parameters and trade-offs

| Setting | Effect |
| --- | --- |
| `ChunkSize` | Bigger chunks hold more context but blur the meaning. Smaller chunks are precise but can lose context. |
| `ChunkOverlap` | More overlap reduces the chance that an answer is split across two chunks, but stores more duplicate text. |

Common starting points are 500 to 1000 characters with about 10 to 20 percent overlap. The right values depend on your documents, and the evaluation lesson later in the course will let you measure them.

## Tests

`server/RagLearn.Api.Tests/MarkdownChunkerTests.cs` checks heading splits, paragraph packing, hard splits, and argument validation. Run them with `dotnet test server/RagLearn.Api.Tests`.
