namespace Nodal.FraudLab.Web.Domain;

public sealed record DeviceFingerprint(
    string Id,
    string DeviceType,
    string OperatingSystem,
    string TrustLevel);
