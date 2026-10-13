# Lesson 2: Documents and ingestion

## Steps

1. Look at the sample files in `data/docs/`. Open one in a text editor and notice the `#` headings.
2. Click **Ingest sample docs**. The API reads each `.md` file and stores it in the `Documents` table.
3. Read the table. Each row is one file, and the chunk count shows how many pieces it was split into.
4. Click **Ingest sample docs** again. The counts should stay the same. The ingest step replaces old data instead of duplicating it.
5. Open **SQLite basics** and run `SELECT * FROM Documents;` to see the same data directly.

## Try this

- Edit a sample file in `data/docs/`, add a paragraph, and ingest again. Check the new character count.
- Add a new `.md` file to `data/docs/`, then ingest. The new file should appear in the table.
- Find the `IngestAllAsync` method in `server/RagLearn.Api/Services/DocumentIngestionService.cs` and trace the steps.
