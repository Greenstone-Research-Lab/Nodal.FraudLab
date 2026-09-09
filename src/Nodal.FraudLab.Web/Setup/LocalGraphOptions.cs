namespace Nodal.FraudLab.Web.Setup;

public sealed class LocalGraphOptions
{
    public const string SectionName = "LocalGraph";

    public Neo4jLocalOptions Neo4j { get; init; } = new();
}

public sealed class Neo4jLocalOptions
{
    public string Host { get; init; } = "localhost";

    public int BoltPort { get; init; } = 7687;

    public string BrowserUrl { get; init; } = "http://localhost:7474";

    public string Username { get; init; } = "neo4j";

    public string Password { get; init; } = "NodalLocal123!";
}
