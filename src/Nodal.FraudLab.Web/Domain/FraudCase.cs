namespace Nodal.FraudLab.Web.Domain;

public sealed record FraudCase(
    string Id,
    string TransactionId,
    string Status,
    decimal RiskScore,
    DateTimeOffset CreatedAt,
    IReadOnlyList<RiskSignal> Signals);
