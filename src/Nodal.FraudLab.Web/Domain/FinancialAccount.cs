namespace Nodal.FraudLab.Web.Domain;

public sealed record FinancialAccount(
    string Id,
    string CustomerId,
    string AccountType,
    DateOnly OpenedOn,
    string CurrencyCode);
