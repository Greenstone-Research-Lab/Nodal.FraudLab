namespace Nodal.FraudLab.Web.Application.Import;

public interface ICsvImportPreviewService
{
    IReadOnlyList<CsvModelDefinition> SupportedModels { get; }

    Task<CsvDataSetPreview> PreviewAsync(
        Stream content,
        string fileName,
        FraudDataSetKind expectedDataSet,
        CancellationToken cancellationToken = default);
}
