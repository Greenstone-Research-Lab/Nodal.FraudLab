namespace Nodal.FraudLab.Web.Setup;

public sealed class LocalPostgresOptions
{
    public const string SectionName = "LocalPostgres";

    public PostgresLocalOptions PostgreSql { get; init; } = new();
}

public sealed class PostgresLocalOptions
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5433;

    public string Database { get; init; } = "fraudlab";

    public string Username { get; init; } = "fraudlab";

    public string Password { get; init; } = "NodalLocal123!";
}
