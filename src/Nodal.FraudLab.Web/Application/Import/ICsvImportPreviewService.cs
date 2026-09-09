namespace Nodal.FraudLab.Web.Application.Import;

public interface ICsvImportPreviewService
{
    Task<CsvDataSetPreview> PreviewAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
}
