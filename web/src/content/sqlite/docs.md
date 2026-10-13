# SQLite for SQL Server developers

SQLite is a database engine that runs inside your application. Your data lives in one ordinary file, and there is no server process to install, start, or log in to. This project stores everything in `data/rag_learn.db`.

## SQLite vs SQL Server

| Topic | SQL Server | SQLite |
| --- | --- | --- |
| Architecture | Client/server. A service listens on a port. | Library. The app reads and writes the file directly. |
| Storage | Many files managed by the server, plus a transaction log. | One file (plus a `-wal` and `-shm` file while in use). |
| Authentication | Logins, roles, permissions. | None. Anyone who can read the file can read the data. |
| Concurrency | Many writers, row-level locking. | One writer at a time. Many readers at the same time. |
| Data types | Strict: `int`, `nvarchar(255)`, `datetime2`, `bit`... | Flexible: stores values as one of five storage classes (see below). |
| Stored procedures, triggers, views | Yes | Views and triggers yes. No stored procedures. |
| Typical use | Large business systems, many users. | Desktop and mobile apps, local tools, prototypes, small web apps, caches. |
| Management tools | SSMS, Azure Data Studio | DB Browser for SQLite, the `sqlite3` command line, or code. |

For this project SQLite is a good fit: there is one user, one file, and no infrastructure to manage. You can copy the `.db` file to another machine and it still works.

## How it works

- **Transactions:** SQLite is fully ACID. A `SaveChanges` call either commits everything or nothing.
- **Locking:** a write locks the whole database. That is fine here, because ingestion is rare.
- **Journal mode:** by default SQLite uses a rollback journal. Write-Ahead Logging (WAL) is faster for mixed reads and writes. You can turn it on with `PRAGMA journal_mode=WAL;`.
- **Schema storage:** the table definitions are stored in a special table called `sqlite_master`. You can query it like any other table.

## Type affinity

SQLite does not have a strict type system. Each column has an affinity, which is a suggestion for how values are stored. The storage classes are:

- `NULL`
- `INTEGER`
- `REAL`
- `TEXT`
- `BLOB`

Common mappings from SQL Server:

| SQL Server | SQLite | Used in this project |
| --- | --- | --- |
| `int` | `INTEGER` | `Id`, `Position`, `CharCount` |
| `nvarchar(255)` | `TEXT` | `FileName`, `Title` |
| `nvarchar(max)` | `TEXT` | `Content` |
| `datetime2` | `TEXT` (ISO-8601 string) | `IngestedAtUtc` |
| `bit` | `INTEGER` (0 or 1) | not used yet |
| `float` | `REAL` | embeddings later |
| `varbinary(max)` | `BLOB` | embeddings later |

The `VARCHAR(255)` length is accepted, but SQLite does not enforce it. EF Core still writes it in the schema for you.

## Basic syntax

Most statements look like SQL Server. Differences are noted in the comments below.

```sql
-- Create a table (SQL Server: CREATE TABLE with IDENTITY)
CREATE TABLE Notes (
    Id      INTEGER PRIMARY KEY AUTOINCREMENT,  -- INTEGER PRIMARY KEY is the rowid alias
    Title   TEXT NOT NULL,
    Body    TEXT,
    Created TEXT DEFAULT (datetime('now'))      -- SQLite date functions, not GETUTCDATE()
);

-- Insert rows
INSERT INTO Notes (Title, Body) VALUES ('First', 'Hello SQLite');
INSERT INTO Notes (Title) VALUES ('Second'), ('Third');

-- Read rows
SELECT Id, Title FROM Notes WHERE Title LIKE 'S%' ORDER BY Id;
SELECT * FROM Notes LIMIT 2;                    -- SQL Server: SELECT TOP 2

-- Update and delete
UPDATE Notes SET Body = 'Edited' WHERE Id = 2;
DELETE FROM Notes WHERE Id = 3;

-- Aggregates and joins work the same way
SELECT COUNT(*) AS Total FROM Notes;
SELECT d.Title, COUNT(c.Id) AS Chunks
FROM Documents d
LEFT JOIN Chunks c ON c.DocumentId = d.Id
GROUP BY d.Title;
```

Things that behave differently from SQL Server:

- Strings use single quotes. Double quotes name identifiers, and SQLite accepts them as strings for backward compatibility, which can hide typos.
- There is no `GETDATE()`. Use `datetime('now')` or pass the timestamp from C#.
- Pagination uses `LIMIT n OFFSET m` instead of `OFFSET ... FETCH`.
- `||` concatenates strings. `+` does arithmetic only.
- Boolean values are 0 and 1.

## PRAGMA

`PRAGMA` commands read or change database settings. They are SQLite-specific and useful for exploring:

```sql
PRAGMA table_info('Chunks');           -- columns of a table
PRAGMA index_list('Chunks');           -- indexes on a table
PRAGMA foreign_keys = ON;              -- enforce foreign keys for this connection
PRAGMA journal_mode;                   -- current journal mode
```

Foreign key enforcement is off by default in SQLite, and it is a per-connection setting. EF Core turns it on for you.

## Interacting with the database

### 1. The in-app SQL console

Open the **SQLite basics** lesson. It sends your query to the API, which opens the file in read-only mode. This is the quickest way to experiment.

### 2. The sqlite3 command line

Install the tools, for example with `winget install SQLite.SQLite`. If that package name is not available, download the `sqlite-tools` bundle from sqlite.org. Then open the database:

```bash
sqlite3 data/rag_learn.db
```

Inside the prompt:

```
.tables            -- list tables
.schema Chunks     -- show CREATE statement
.headers on        -- show column names
.mode column       -- align output
SELECT * FROM Documents;
.quit
```

Use this for writes too, such as `DELETE FROM Chunks;`. The app is still running while you do this, so keep the changes small.

### 3. DB Browser for SQLite

A free GUI, similar in spirit to SSMS. Open `data/rag_learn.db`, browse tables, edit rows, and run SQL in the Execute SQL tab.

### 4. From C# with EF Core

The code you already write works unchanged:

```csharp
var docs = await db.Documents.Where(d => d.FileName.EndsWith(".md")).ToListAsync();
```

Only the provider line is different: `options.UseSqlite("Data Source=...")` in `Program.cs`.

## Read-only connections

The SQL console uses this connection string:

```csharp
new SqliteConnectionStringBuilder
{
    DataSource = path,
    Mode = SqliteOpenMode.ReadOnly
}
```

With `Mode=ReadOnly`, SQLite rejects any `INSERT`, `UPDATE`, `DELETE`, or `CREATE` before it touches the file. This is safer than trying to parse the SQL yourself. Try it in the console.

## Quick reference

| Task | Command |
| --- | --- |
| List tables | `SELECT name FROM sqlite_master WHERE type = 'table';` |
| Show a table's columns | `PRAGMA table_info('Chunks');` |
| Count rows | `SELECT COUNT(*) FROM Chunks;` |
| Find the newest row | `SELECT * FROM Documents ORDER BY IngestedAtUtc DESC LIMIT 1;` |
| Delete all chunks | `DELETE FROM Chunks;` (ingest again to restore them) |
| Reset the database | Stop the API and delete `data/rag_learn.db`. It is recreated on the next start. |
