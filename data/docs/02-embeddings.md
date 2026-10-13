# Embeddings

An embedding is a list of numbers, called a vector, that represents the meaning of a piece of text. Texts with similar meaning produce vectors that point in similar directions.

## Similarity

Similarity between two vectors is usually measured with cosine similarity. A value close to 1 means the texts are very similar, a value near 0 means they are unrelated. Cosine similarity ignores vector length and compares only direction.

## Dimensions

An embedding model outputs a fixed number of dimensions, for example 1536 or 3072. More dimensions can capture more nuance but cost more storage and computation. Every vector in the same collection must come from the same model, because vectors from different models are not comparable.

## Storage

For a small project, you can store each embedding as a BLOB of floating-point numbers in SQLite and compute cosine similarity in code. Large collections need a dedicated vector index, which is covered in the advanced lessons.
