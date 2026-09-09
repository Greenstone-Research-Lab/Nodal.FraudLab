namespace Nodal.FraudLab.Web.Domain;

public sealed record RiskSignal(
    string Code,
    string Title,
    string Description,
    decimal Score);
