# 6. Retrieval

Retrieval answers the question "which chunks are most relevant to this query?". Here, retrieval uses **vector search** only: embed the query, compare it with every chunk embedding, and keep the top K.

## Steps

1. Complete lesson 5 so that every chunk has an embedding.
2. In the **Search** panel, type a question such as `How does overlap help chunking?`.
3. Leave **Vector** on and turn **BM25** off. This makes it pure vector search.
4. Click **Search** and read the results. The `vector #` and `cosine` values show where each chunk ranked and how similar it was.
5. Try a filter: pick one file in **Document** and search again. Only that file's chunks are candidates.
6. Read the **Pipeline** list under the results. It shows each stage in order: filter, ranking, and the final cut.

## Try this

- Search for a word that does not appear in the documents, such as `kubernetes`. Vector search still returns chunks, ranked by meaning, even though none contains the word. Lesson 8 shows where keyword search fits in.
- Lower **Top K** to 1 and compare the top result with the one you get at 5.
- Search for a paraphrase, such as `splitting text into pieces`, and check whether the chunking lesson's chunk ranks first.

## What to notice

- The filter runs in SQL before any math. Only chunks that pass the filter are compared with the query vector.
- The score is brute force: every candidate is compared. That is fine for a few hundred chunks.
- Results are sorted by score, with the chunk ID as a tie-breaker so the order is stable.
