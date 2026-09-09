using System.Net.Sockets;

namespace Nodal.FraudLab.Web.Setup;

public static class Neo4jConnectionProbe
{
    public static async Task<Neo4jConnectionProbeResult> ProbeAsync(
        Neo4jLocalOptions options,
        CancellationToken cancellationToken)
    {
        using var client = new TcpClient();

        try
        {
            await client.ConnectAsync(options.Host, options.BoltPort, cancellationToken);
            return new Neo4jConnectionProbeResult(true, options.Host, options.BoltPort,
                "The local Neo4j Bolt endpoint is reachable.");
        }
        catch (Exception exception) when (exception is SocketException or OperationCanceledException)
        {
            return new Neo4jConnectionProbeResult(false, options.Host, options.BoltPort,
                "Start Docker Desktop and run the local Compose command before retrying.");
        }
    }
}

public sealed record Neo4jConnectionProbeResult(bool IsReachable, string Host, int Port, string Message);
