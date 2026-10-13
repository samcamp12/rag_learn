# Lesson 3: SQLite basics

SQLite stores this project's data in one file, `data/rag_learn.db`. Read the **Docs** tab first for the SQL Server comparison, then practice here.

## Steps

1. Click **List tables**, then **Run query**. You should see `Documents` and `Chunks`. If the list is empty, ingest the sample files in lesson 2 first.
2. Click **Documents**. Each row is one Markdown file.
3. Click **Chunks per document**. Notice the `LEFT JOIN`, which is the same join syntax you use in SQL Server.
4. Click **Table schema**. This runs `PRAGMA table_info`, which lists the columns and their types.
5. Write your own query. Try `SELECT Heading, CharCount FROM Chunks WHERE CharCount > 300;`.
6. Try a write, for example `DELETE FROM Chunks;`. The console is read-only, so SQLite refuses it. The error message shows the read-only connection working.

## Try the command line (optional)

Install the `sqlite3` tool (see the Docs tab), then run `sqlite3 data/rag_learn.db` from the repository root. Run `.tables`, then `.schema Chunks`.

## Questions to answer

- Which column is the primary key of `Chunks`? How do you know?
- What does `ON DELETE CASCADE` mean for the link between `Chunks` and `Documents`?
- Why does `CharCount` exist when `length(Content)` could calculate it?
