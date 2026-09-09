namespace Nodal.FraudLab.Web.Application.Import;

public sealed record CsvModelDefinition(
    FraudDataSetKind DataSet,
    string DisplayName,
    string DemoFileName,
    IReadOnlyList<CsvModelPropertyDefinition> Properties);
