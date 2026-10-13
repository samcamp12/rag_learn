# Project map

```
rag_learn/
├── data/
│   ├── docs/                  Sample Markdown files that get ingested
│   └── rag_learn.db           SQLite database (created on first run, not committed)
├── server/
│   ├── RagLearn.slnx          Solution file that groups both projects
│   ├── RagLearn.Api/          ASP.NET Core Web API
│   │   ├── Program.cs         Startup: services, database, endpoints
│   │   ├── appsettings.json   Non-secret configuration (paths, chunk sizes, model names)
│   │   ├── Endpoints/         HTTP routes (ApiEndpoints.cs)
│   │   ├── Services/          Chunker and ingestion logic
│   │   ├── Data/              EF Core DbContext
│   │   ├── Models/            Document and Chunk entities
│   │   └── Options/           Strongly typed configuration classes
│   └── RagLearn.Api.Tests/    xUnit tests
└── web/                       React + Vite frontend
    ├── vite.config.ts         Dev server and /api proxy
    └── src/
        ├── App.tsx            Three-column layout and lesson switching
        ├── lessons.ts         Lesson list and Markdown content imports
        ├── api.ts             Typed calls to the backend
        ├── components/        Left sidebar, right panel, Markdown renderer
        ├── pages/             One component per lesson
        ├── content/           Instructions and Docs for each lesson
        └── styles.css         Layout and theme
```

## Request path for one button click

Clicking **Ingest sample docs** runs:

1. `DocumentsLesson.tsx` calls `api.ingest()`.
2. `api.ts` sends `POST /api/ingest`.
3. `ApiEndpoints.cs` receives it and resolves `DocumentIngestionService`.
4. The service reads files, runs `MarkdownChunker`, and calls `SaveChangesAsync`.
5. EF Core writes the rows to `rag_learn.db`, and the JSON result returns to the page.

Follow this path with the source files open. It covers most of what you need to understand this codebase.
