namespace Nodal.FraudLab.Web.Contracts;

public sealed record FraudDashboardSummary(
    int ConnectedSources,
    int OpenCases,
    int HighRiskCases,
    int EvidencePaths);
