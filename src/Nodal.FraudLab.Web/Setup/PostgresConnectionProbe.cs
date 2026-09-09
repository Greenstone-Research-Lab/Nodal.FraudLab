using System.Net.Sockets;

namespace Nodal.FraudLab.Web.Setup;

public static class PostgresConnectionProbe
{
    public static async Task<PostgresConnectionProbeResult> ProbeAsync(
        PostgresLocalOptions options,
        CancellationToken cancellationToken)
    {
        using var client = new TcpClient();

        try
        {
            await client.ConnectAsync(options.Host, options.Port, cancellationToken);
            return new PostgresConnectionProbeResult(true, options.Host, options.Port,
                "The local PostgreSQL endpoint is reachable.");
        }
        catch (Exception exception) when (exception is SocketException or OperationCanceledException)
        {
            return new PostgresConnectionProbeResult(false, options.Host, options.Port,
                "Start Docker Desktop and run the local Compose command before retrying.");
        }
    }
}

public sealed record PostgresConnectionProbeResult(bool IsReachable, string Host, int Port, string Message);
