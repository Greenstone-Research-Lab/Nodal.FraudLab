namespace Nodal.FraudLab.Web.Domain;

public sealed record FinancialTransaction(
    string Id,
    string AccountId,
    string MerchantId,
    decimal Amount,
    string CurrencyCode,
    DateTimeOffset OccurredAt,
    string Status);
