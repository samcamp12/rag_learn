# Embeddings

## What an embedding is

An embedding is a fixed-length array of floats produced by a neural network. `text-embedding-3-small` returns 1536 numbers for any input up to about 8,000 tokens. The numbers have no meaning on their own. What matters is the geometry: two texts about the same idea land close together, and unrelated texts land far apart.

```
"How do I chunk a document?"   -> [0.021, -0.113, 0.407, ... 1536 numbers]
"Splitting text into pieces"   -> [0.018, -0.101, 0.395, ... 1536 numbers]   (close)
"SQL Server index tuning"      -> [-0.240, 0.062, -0.019, ... 1536 numbers] (far)
```

## Similarity: cosine

To compare two vectors we use **cosine similarity**: the cosine of the angle between them.

- `1.0` means the same direction (same meaning).
- `0.0` means unrelated (perpendicular).
- The length of the vector is ignored, which is why it works well for text.

The formula is `dot(a, b) / (|a| * |b|)`. `VectorMath.CosineSimilarity` in `Services/Retrieval/VectorMath.cs` implements it with plain loops.

## Storing vectors in SQLite

SQLite has no vector type. We store each vector as a **BLOB** (binary large object) of raw little-endian float32 bytes:

- 1536 floats x 4 bytes = 6,144 bytes per chunk.
- `VectorMath.ToBytes` writes the bytes; `VectorMath.FromBytes` reads them back.
- Only the bytes are stored, so the column does not know the dimensions. The `EmbeddingModel` column records which model produced them.

## C# and SQL Server comparison

| Concept | C# / SQL Server | RAG / SQLite |
|---|---|---|
| Fixed-size numeric array | `float[]`, `real[]` | `float[]` in C#, BLOB in SQLite |
| Binary column | `varbinary(max)` | `BLOB`, read as `byte[]` |
| Similarity search | Custom code or `VECTOR` type (SQL Server 2025 preview) | Brute-force cosine in C# |
| Mapping a binary column in EF Core | `byte[]` property | `byte[]? Embedding` on `Chunk` |

Note: SQL Server 2025 added a native `vector` type. SQLite does not have one, so we compute similarity in C#. Lesson 13 covers ways to add a real vector index.

## Why we embed the chunks, not the documents

Embedding a whole document blurs many topics into one point. Embedding small chunks (lesson 4) lets a query match the exact paragraph that answers it. The chunk is also the unit we return to the language model later.
