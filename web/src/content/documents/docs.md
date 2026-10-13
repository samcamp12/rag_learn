# Ingestion: reading files and writing rows

Ingestion is the first half of every RAG system. It turns raw files into rows that a search can use later.

## What happens on each ingest

For each `.md` file in the documents folder, `DocumentIngestionService` does four things:

1. Reads the file into a string.
2. Finds or creates a `Document` row keyed by `FileName`.
3. Removes the old chunks for that document and creates new ones from the current text.
4. Saves everything in one `SaveChanges()` call.

## Why one SaveChanges matters

In SQL Server you would wrap the work in a transaction, such as `BEGIN TRANSACTION ... COMMIT`. EF Core does this for you: a single `SaveChangesAsync` call runs all inserts, updates, and deletes inside one transaction. If one step fails, nothing is written. SQLite supports the same guarantee, and it is covered in the SQLite lesson.

## Compared with SQL Server

| Concept | SQL Server | This project |
| --- | --- | --- |
| Unique key on FileName | `CREATE UNIQUE INDEX` | `entity.HasIndex(d => d.FileName).IsUnique()` in `AppDbContext` |
| Delete children when parent is removed | `ON DELETE CASCADE` | `DeleteBehavior.Cascade` |
| Insert a parent and its children | `INSERT` twice, using the new identity value | EF Core fills in `Id` values after `SaveChanges` |
| Identity column | `IDENTITY(1,1)` | `int Id` is an auto-increment key by convention |

## Title extraction

The first `# Heading` in a file becomes its title. If there is none, the file name is used. This is simple string logic, with no regular expressions.

## Important: EnsureCreated

`Program.cs` calls `db.Database.EnsureCreated()`. It creates the tables if the database file does not exist yet. This is fine for learning, but it cannot change an existing schema. Production projects use EF Core migrations, which record each schema change as a file. Migrations are a good follow-up exercise.
