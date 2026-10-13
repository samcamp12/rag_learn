# Architecture overview

The app has three layers. Each one is small enough to read in one sitting.

```
Browser (React + Vite)  ->  ASP.NET Core API  ->  SQLite file
  localhost:5173            localhost:5022        data/rag_learn.db
```

## Request flow

1. A React component calls `api.documents()` in `web/src/api.ts`.
2. The Vite dev server forwards any `/api/...` request to port 5022. This is configured in `web/vite.config.ts`, so the browser never makes a cross-origin call.
3. ASP.NET Core matches the route in `server/RagLearn.Api/Endpoints/ApiEndpoints.cs`.
4. The endpoint asks `AppDbContext` for data. EF Core turns the LINQ query into SQL.
5. SQLite reads the file and returns rows. EF Core maps them to C# objects, and ASP.NET serializes them to JSON.

## Compared with ASP.NET Core

| This project | What you may already know |
| --- | --- |
| `Program.cs` with `WebApplication.CreateBuilder` | The same top-level statements from .NET 6 and later. Services are registered before `Build()`, and the middleware pipeline comes after. |
| `app.MapGroup("/api")` and `MapGet`/`MapPost` | Minimal APIs. They do the job of `[ApiController]` classes, but with lambdas instead of controller methods. |
| `AppDbContext` | The same DbContext pattern you use with SQL Server. Only the provider changes: `UseSqlite(...)` instead of `UseSqlServer(...)`. |
| `AddScoped<DocumentIngestionService>()` | Standard dependency injection. A scoped service lives for one HTTP request, just as in ASP.NET Core. |
| `IOptions<RagOptions>` | The options pattern. It binds a section of `appsettings.json` to a class. |
| `appsettings.json` and user secrets | The same configuration system. Secrets live outside the project folder. |

## Where things live

- `Models/`: entity classes, similar to EF Core entities in any ASP.NET project.
- `Data/`: the DbContext and the table mapping rules.
- `Services/`: business logic such as chunking and ingestion. The chunker is pure code with no database, which makes it easy to test.
- `Endpoints/`: HTTP routes and the DTO records they return.
- `Options/`: strongly typed configuration.
- `RagLearn.Api.Tests/`: xUnit tests. Compare them with any test project you have written.

## Why there is no `.env` file

`.env` files are a Node.js convention. ASP.NET Core does not read them automatically. Instead, .NET uses `appsettings.json`, environment variables, and user secrets. Using the .NET approach keeps the project consistent.
