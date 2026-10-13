# Lesson 4: Chunking

## Steps

1. Pick a document in the dropdown. Start with `03-chunking.md`, since it describes the same ideas you are looking at.
2. Read each chunk. Notice that chunks follow the headings in the original file.
3. Look at the character counts. Most chunks should be under 800 characters, the value set by `ChunkSize`.
4. Open `server/RagLearn.Api/appsettings.json` and change `ChunkSize` to `300` and `ChunkOverlap` to `50`.
5. Restart the API with `dotnet run`, then click **Ingest sample docs** in lesson 2. Come back here and compare the new chunks.

## Things to notice

- Some chunks start with text that already appeared at the end of the previous chunk. That is the overlap.
- Headings are kept as metadata on each chunk, not repeated in the text.
- Very short sections become their own chunk.
