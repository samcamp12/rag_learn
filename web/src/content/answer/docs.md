# Answer generation

## The prompt

A RAG prompt has three parts: instructions, context, and the question.

```
system:  You answer questions using only the numbered sources provided.
         Cite sources inline with their number, like [1].
         If the sources do not contain the answer, say you don't know.

user:    Sources:
         [1] 03-chunking.md > Overlap
         ...chunk text...

         [2] 03-chunking.md > Parameters
         ...chunk text...

         Question: What is chunk overlap and why is it used?
```

`AnswerService.AnswerAsync` builds this prompt in `Services/Retrieval/AnswerService.cs`.

## Calling the model

`OpenAiClient.CompleteAsync` posts to `/v1/chat/completions` using `HttpClient`, a Bearer token, and snake_case JSON. This is the same work `HttpClient` and `System.Text.Json` do in any ASP.NET Core service that calls an external API.

```csharp
var answer = await openAi.CompleteAsync(
[
    new ChatMessage("system", SystemPrompt),
    new ChatMessage("user", $"Sources:\n\n{sources}\n\nQuestion: {question}"),
], false, ct);
```

## C# and ASP.NET Core comparison

| Concept | C# / ASP.NET Core | RAG |
|---|---|---|
| Typed HTTP client | `AddHttpClient<IFoo, Foo>()` | `AddHttpClient<IOpenAiClient, OpenAiClient>()` |
| Options from config | `IOptions<OpenAiOptions>` | `OpenAiOptions` section in `appsettings.json` |
| Secrets | `dotnet user-secrets` | `OpenAI:ApiKey` |
| Service orchestration | A service class that calls repositories and clients | `AnswerService` calls `RetrievalService` then `IOpenAiClient` |
| Error at the boundary | Map exceptions to HTTP status codes | `OpenAiFailure` maps errors to 400 or 502 |

## Grounding and citations

Grounding means the answer must be supported by the retrieved text. Three things help:

1. **Instructions:** tell the model to use only the sources and to say when it does not know.
2. **Numbered sources:** the model can cite `[n]`, and you can check each citation.
3. **Small, relevant context:** a few good chunks work better than many weak ones.

Grounding is not guaranteed. Always check the citations, and treat the sources list as the source of truth.

## What we are not doing yet

- No streaming. The answer appears all at once (lesson 12 adds streaming).
- No conversation memory. Each question stands alone (lesson 12).
- No reranking or query rewriting. Lessons 9 and 10 improve what goes into the prompt.
