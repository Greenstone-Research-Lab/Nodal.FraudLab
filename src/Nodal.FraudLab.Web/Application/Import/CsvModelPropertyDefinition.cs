namespace Nodal.FraudLab.Web.Application.Import;

public sealed record CsvModelPropertyDefinition(
    string Name,
    string DotNetType,
    bool IsRequired,
    string ValidationRule);
