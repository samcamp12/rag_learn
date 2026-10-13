# Lesson 1: Run the project

## Start the backend

Open a terminal in `server/RagLearn.Api` and run:

```bash
dotnet run
```

The API listens on `http://localhost:5022`. Open `http://localhost:5022/api/health` in a browser to confirm it works.

## Start the frontend

In a second terminal, open `web` and run:

```bash
npm install
npm run dev
```

Open `http://localhost:5173`. The sidebar on the left shows the backend status. The panel on the right explains the current lesson.

## Add your OpenAI key (later)

The key is intentionally blank. When you are ready, store it with .NET user secrets so it never ends up in a file you might commit:

```bash
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
```

Run that command inside `server/RagLearn.Api`. Check the status panel on this page to confirm the key was picked up.

## Run the tests

```bash
dotnet test server/RagLearn.Api.Tests
```

## What to try now

1. Click **Refresh** on this page and read each value.
2. Open **Documents & ingestion** and load the sample Markdown files.
3. Open the **Docs** tab on the right to learn how the pieces fit together.
