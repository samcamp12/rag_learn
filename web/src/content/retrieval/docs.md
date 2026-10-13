# Retrieval

## The pipeline

```
query text
  -> embed the query (OpenAI /v1/embeddings)
  -> filter chunks in SQL (file, date, heading)           [WHERE clause]
  -> score each candidate: cosine(query vector, chunk vector)
  -> sort by score descending, keep top K
  -> return chunks with their scores
```

`RetrievalService.RetrieveAsync` in `Services/Retrieval/RetrievalService.cs` runs these steps. The response includes a `stages` list so you can see what happened.

## Brute force versus an index

We compare the query against every candidate. This is an O(n) scan, the same kind of work SQL Server does for a table scan with no useful index. For a few hundred chunks it takes milliseconds.

Larger collections need an **approximate nearest neighbor** index such as HNSW or IVF. That trades a small amount of accuracy for speed. Lesson 13 discusses options.

## Filters before ranking

Metadata filters are applied first, in SQL, with `WHERE`:

```csharp
query = query.Where(c => c.Document!.FileName == fileName);
```

This is the same as a SQL Server `WHERE` clause on a joined table. Filtering first means fewer comparisons and no chance that a filtered-out chunk ranks in the results.

## C# and SQL Server comparison

| Concept | C# / SQL Server | RAG |
|---|---|---|
| Filter then sort | `WHERE ... ORDER BY score DESC` | Same, with score computed in C# |
| Top N | `SELECT TOP (@k)` or `.Take(k)` | `.Take(TopK)` after sorting |
| Scalar function per row | Scalar UDF or CLR function | `CosineSimilarity` per candidate |
| Stable order on ties | `ORDER BY score DESC, Id` | `.ThenBy(s => s.ChunkId)` |
| Query plan | `SET STATISTICS IO` | The `stages` list in the response |

## Why the score matters

Cosine scores are not probabilities. A score of 0.45 is not "45% correct". Only the ordering is meaningful, and the absolute value depends on the model. That is why we show the rank next to the score.

## Pure vector search has limits

Vector search finds meaning, but it can miss exact terms such as error codes, function names, or IDs. Lesson 8 combines it with keyword ranking (BM25) using Reciprocal Rank Fusion.
