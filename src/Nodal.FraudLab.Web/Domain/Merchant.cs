namespace Nodal.FraudLab.Web.Domain;

public sealed record Merchant(
    string Id,
    string Name,
    string MerchantCategoryCode,
    string CountryCode);
