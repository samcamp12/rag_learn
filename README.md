# RAG Learn

A hands-on project for learning Retrieval-Augmented Generation (RAG). It has a React frontend, an ASP.NET Core API, and a SQLite database file.

## Prerequisites

- .NET 10 SDK
- Node.js 18 or newer (Node 20 or 22 LTS is recommended)

## Quick start

```bash
# Terminal 1: backend on http://localhost:5022
cd server/RagLearn.Api
dotnet run

# Terminal 2: frontend on http://localhost:5173
cd web
npm install
npm run dev
```

Open http://localhost:5173 and start with lesson 1.

## Tests

```bash
dotnet test server/RagLearn.slnx
```

## OpenAI key

The key is not set yet. When you are ready, store it with .NET user secrets:

```bash
cd server/RagLearn.Api
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
```

User secrets are stored outside the repository, so the key is never committed.

## Learning path

The app itself is the course. The left sidebar lists the lessons, the right panel shows instructions, concept docs, and a project map. Lessons 5 to 7 (embeddings, retrieval, and answer generation) come next.
