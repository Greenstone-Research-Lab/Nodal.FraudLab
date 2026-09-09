# Nodal FraudLab

Public Blazor Web App starter for a graph-evidence fraud-detection proof of
concept.

## Current scope

- .NET 10 Blazor Web App with interactive server rendering;
- domain models for financial actors, transactions, devices, addresses, risk
  signals, and fraud cases;
- application and repository contracts;
- a Docker Compose local Neo4j profile, a reachability probe endpoint, and a
  dashboard shell;
- setup, data-upload, operations-and-analysis, and graph-visualisation pages.

No graph provider, Nodal Framework package, source connection, persistence,
or fraud-detection algorithm is implemented yet. Those are intentionally the
next hands-on implementation steps.

## Local run

```powershell
docker compose -f .\docker-compose.local.yml up -d
Test-NetConnection localhost -Port 7687
dotnet restore .\Nodal.FraudLab.slnx
dotnet run --project .\src\Nodal.FraudLab.Web
```

The local Neo4j Browser is available at `http://localhost:7474` with the
disposable development credentials `neo4j` / `NodalLocal123!`. Do not reuse
these credentials outside local development.

`/api/health` is available without a data source. The Bolt reachability probe
is available at `/api/setup/neo4j-connection`.
