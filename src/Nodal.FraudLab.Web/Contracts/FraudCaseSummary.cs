namespace Nodal.FraudLab.Web.Contracts;

public sealed record FraudCaseSummary(
    string CaseId,
    string Status,
    decimal RiskScore,
    DateTimeOffset CreatedAt,
    string PrimarySignal);
