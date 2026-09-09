using Microsoft.AspNetCore.DataProtection;
using Nodal.FraudLab.Web.Application.Import;
using Nodal.FraudLab.Web.Components;
using Nodal.FraudLab.Web.Setup;

var builder = WebApplication.CreateBuilder(args);
var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, ".local", "data-protection-keys");

Directory.CreateDirectory(dataProtectionPath);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("Nodal.FraudLab");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<ICsvImportPreviewService, CsvImportPreviewService>();

var app = builder.Build();
var localGraph = app.Configuration.GetSection(LocalGraphOptions.SectionName).Get<LocalGraphOptions>()
    ?? throw new InvalidOperationException("The LocalGraph configuration section is required.");
var localPostgres = app.Configuration.GetSection(LocalPostgresOptions.SectionName).Get<LocalPostgresOptions>()
    ?? throw new InvalidOperationException("The LocalPostgres configuration section is required.");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapGet("/api/health", () => Results.Ok(new
{
    Service = "Nodal.FraudLab.Web",
    Status = "Template",
    UtcNow = DateTimeOffset.UtcNow,
}));

app.MapGet("/api/setup/neo4j-connection", async (CancellationToken cancellationToken) =>
{
    var result = await Neo4jConnectionProbe.ProbeAsync(localGraph.Neo4j, cancellationToken);
    return result.IsReachable ? Results.Ok(result) : Results.Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "Neo4j Bolt endpoint is not reachable.",
        detail: result.Message);
});

app.MapGet("/api/setup/postgres-connection", async (CancellationToken cancellationToken) =>
{
    var result = await PostgresConnectionProbe.ProbeAsync(localPostgres.PostgreSql, cancellationToken);
    return result.IsReachable ? Results.Ok(result) : Results.Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "PostgreSQL endpoint is not reachable.",
        detail: result.Message);
});

app.MapGet("/api/fraud-cases", () => Results.StatusCode(StatusCodes.Status501NotImplemented));
app.MapGet("/api/graph-evidence", () => Results.StatusCode(StatusCodes.Status501NotImplemented));

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
