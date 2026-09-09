namespace Nodal.FraudLab.Web.Domain;

public sealed record CustomerProfile(
    string Id,
    string DisplayName,
    DateOnly OnboardedOn,
    string CountryCode);
