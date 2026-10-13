namespace RagLearn.Api.Services.Retrieval;

public record RetrievalFilter(
    string? FileName = null,
    DateTime? IngestedFromUtc = null,
    DateTime? IngestedToUtc = null,
    string? SectionContains = null);

public record RetrievalOptions(
    int TopK = 5,
    bool UseVector = true,
    bool UseBm25 = true,
    RetrievalFilter? Filter = null);

public record RetrievedChunk(
    int ChunkId,
    int DocumentId,
    string FileName,
    string Title,
    string? Heading,
    string Content,
    int Rank,
    double Score,
    int? VectorRank,
    double? VectorScore,
    int? Bm25Rank);

public record RetrievalResult(
    IReadOnlyList<RetrievedChunk> Chunks,
    int CandidateCount,
    int EmbeddedCandidateCount,
    IReadOnlyList<string> Stages);
