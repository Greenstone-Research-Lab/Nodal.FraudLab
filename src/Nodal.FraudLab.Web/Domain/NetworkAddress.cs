namespace Nodal.FraudLab.Web.Domain;

public sealed record NetworkAddress(
    string Id,
    string CountryCode,
    string NetworkType);
