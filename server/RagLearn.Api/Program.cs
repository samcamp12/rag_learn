using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RagLearn.Api.Data;
using RagLearn.Api.Endpoints;
using RagLearn.Api.Options;
using RagLearn.Api.Services;
using RagLearn.Api.Services.OpenAi;
using RagLearn.Api.Services.Retrieval;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RagOptions>(builder.Configuration.GetSection(RagOptions.SectionName));
builder.Services.PostConfigure<RagOptions>(options =>
{
    options.DocsPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, options.DocsPath));
    options.DatabasePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, options.DatabasePath));
});
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var rag = serviceProvider.GetRequiredService<IOptions<RagOptions>>().Value;
    options.UseSqlite($"Data Source={rag.DatabasePath}");
});

builder.Services.AddScoped<DocumentIngestionService>();
builder.Services.AddHttpClient<IOpenAiClient, OpenAiClient>();
builder.Services.AddScoped<EmbeddingIndexer>();
builder.Services.AddScoped<RetrievalService>();
builder.Services.AddScoped<AnswerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var rag = scope.ServiceProvider.GetRequiredService<IOptions<RagOptions>>().Value;
    Directory.CreateDirectory(Path.GetDirectoryName(rag.DatabasePath)!);

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapRagEndpoints();
app.MapRetrievalEndpoints();

app.Run();
