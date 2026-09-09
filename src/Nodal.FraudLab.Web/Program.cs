using Nodal.FraudLab.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

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

app.MapGet("/api/fraud-cases", () => Results.StatusCode(StatusCodes.Status501NotImplemented));
app.MapGet("/api/graph-evidence", () => Results.StatusCode(StatusCodes.Status501NotImplemented));

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
