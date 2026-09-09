# Nodal FraudLab

Public Blazor Web App starter for a graph-evidence fraud-detection proof of
concept.

## Current scope

- .NET 10 Blazor Web App with interactive server rendering;
- domain models for financial actors, transactions, devices, addresses, risk
  signals, and fraud cases;
- application and repository contracts;
- placeholder API endpoints and a dashboard shell.

No graph provider, Nodal Framework package, source connection, persistence,
or fraud-detection algorithm is implemented yet. Those are intentionally the
next hands-on implementation steps.

## Local run

```powershell
dotnet restore .\Nodal.FraudLab.slnx
dotnet run --project .\src\Nodal.FraudLab.Web
```

`/api/health` is available without a data source.
