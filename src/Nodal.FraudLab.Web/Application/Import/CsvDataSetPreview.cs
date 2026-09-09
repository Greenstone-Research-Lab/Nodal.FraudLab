namespace Nodal.FraudLab.Web.Application.Import;

public sealed record CsvDataSetPreview(
    string FileName,
    FraudDataSetKind DataSet,
    int TotalRows,
    int ValidRows,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, string>> PreviewRows,
    IReadOnlyList<CsvImportIssue> Issues)
{
    public int InvalidRows => Issues.Count(issue => issue.RowNumber.HasValue);

    public bool IsReadyForImport => DataSet != FraudDataSetKind.Unknown && Issues.Count == 0 && ValidRows > 0;
}
