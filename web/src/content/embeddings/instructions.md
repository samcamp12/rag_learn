# 5. Embeddings

An **embedding** turns a piece of text into a list of numbers (a vector). Texts with similar meaning get vectors that point in similar directions, so "how do I find related text?" becomes "which vectors are closest to this one?".

## Steps

1. Make sure you have ingested the documents (lesson 2) and that the **Status** line shows `chunkCount > 0`.
2. Check the **Embedded chunks** counter. It should be `0 / N`.
3. Click **Embed all chunks**. The backend sends each chunk's text to OpenAI's `/v1/embeddings` endpoint in batches of 64, then stores the result in the `Chunks` table.
4. The counter should now read `N / N`.
5. Open the SQL console in lesson 3 and run:

```sql
SELECT Id, Position, EmbeddingModel, length(Embedding) AS bytes
FROM Chunks
LIMIT 5;
```

Each row should show `bytes = 6144`. That is 1536 floats at 4 bytes each.

## What to notice

- The model is `text-embedding-3-small`, set in `appsettings.json`. Every chunk records the model that created its vector.
- If you change the model, the stored vectors no longer match the new model's space. The embed step re-embeds any chunk whose `EmbeddingModel` differs from the current setting.
- Re-ingesting a document deletes its chunks, so the new chunks have no embedding until you embed again.
