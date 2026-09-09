using System.Globalization;
using System.Text;

namespace Nodal.FraudLab.Web.Application.Import;

public sealed class CsvImportPreviewService : ICsvImportPreviewService
{
    private static readonly IReadOnlyList<CsvSchemaDefinition> Schemas =
    [
        new(
            FraudDataSetKind.CustomerProfile,
            ["Id", "DisplayName", "OnboardedOn", "CountryCode"],
            [new("Id"), new("DisplayName"), new("OnboardedOn", IsDateOnly), new("CountryCode")]),
        new(
            FraudDataSetKind.FinancialAccount,
            ["Id", "CustomerId", "AccountType", "OpenedOn", "CurrencyCode"],
            [new("Id"), new("CustomerId"), new("AccountType"), new("OpenedOn", IsDateOnly), new("CurrencyCode")]),
        new(
            FraudDataSetKind.Merchant,
            ["Id", "Name", "MerchantCategoryCode", "CountryCode"],
            [new("Id"), new("Name"), new("MerchantCategoryCode"), new("CountryCode")]),
        new(
            FraudDataSetKind.DeviceFingerprint,
            ["Id", "DeviceType", "OperatingSystem", "TrustLevel"],
            [new("Id"), new("DeviceType"), new("OperatingSystem"), new("TrustLevel")]),
        new(
            FraudDataSetKind.NetworkAddress,
            ["Id", "CountryCode", "NetworkType"],
            [new("Id"), new("CountryCode"), new("NetworkType")]),
        new(
            FraudDataSetKind.FinancialTransaction,
            ["Id", "AccountId", "MerchantId", "Amount", "CurrencyCode", "OccurredAt", "Status"],
            [new("Id"), new("AccountId"), new("MerchantId"), new("Amount", IsDecimal), new("CurrencyCode"), new("OccurredAt", IsDateTimeOffset), new("Status")]),
    ];

    public async Task<CsvDataSetPreview> PreviewAsync(
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return CreateUnknownPreview(fileName, [], [new(null, "The CSV file does not contain a header row.")]);
        }

        var columns = ParseLine(headerLine).Select(column => column.Trim()).ToArray();
        var schema = Schemas.SingleOrDefault(candidate => candidate.Matches(columns));
        if (schema is null)
        {
            return CreateUnknownPreview(
                fileName,
                columns,
                [new(null, "The column set does not match a supported FraudLab domain model.")]);
        }

        var previewRows = new List<IReadOnlyDictionary<string, string>>();
        var issues = new List<CsvImportIssue>();
        var totalRows = 0;
        var validRows = 0;
        var rowNumber = 1;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            totalRows++;
            var fields = ParseLine(line);
            if (fields.Count != columns.Length)
            {
                issues.Add(new CsvImportIssue(rowNumber, $"Expected {columns.Length} values but found {fields.Count}."));
                continue;
            }

            var values = columns
                .Select((column, index) => new KeyValuePair<string, string>(column, fields[index].Trim()))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

            var rowIssues = schema.Validate(values);
            if (rowIssues.Count > 0)
            {
                issues.AddRange(rowIssues.Select(message => new CsvImportIssue(rowNumber, message)));
                continue;
            }

            validRows++;
            if (previewRows.Count < 5)
            {
                previewRows.Add(values);
            }
        }

        if (totalRows == 0)
        {
            issues.Add(new CsvImportIssue(null, "The CSV file does not contain any data rows."));
        }

        return new CsvDataSetPreview(fileName, schema.DataSet, totalRows, validRows, columns, previewRows, issues);
    }

    private static CsvDataSetPreview CreateUnknownPreview(
        string fileName,
        IReadOnlyList<string> columns,
        IReadOnlyList<CsvImportIssue> issues) =>
        new(fileName, FraudDataSetKind.Unknown, 0, 0, columns, [], issues);

    private static bool IsDateOnly(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private static bool IsDecimal(string value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _);

    private static bool IsDateTimeOffset(string value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _);

    private static List<string> ParseLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var isQuoted = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (isQuoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                    continue;
                }

                isQuoted = !isQuoted;
                continue;
            }

            if (character == ',' && !isQuoted)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString());
        return fields;
    }

    private sealed record CsvSchemaDefinition(
        FraudDataSetKind DataSet,
        IReadOnlyList<string> ExpectedColumns,
        IReadOnlyList<CsvFieldDefinition> Fields)
    {
        public bool Matches(IReadOnlyList<string> columns) =>
            columns.Count == ExpectedColumns.Count &&
            columns.All(column => ExpectedColumns.Contains(column, StringComparer.OrdinalIgnoreCase));

        public IReadOnlyList<string> Validate(IReadOnlyDictionary<string, string> values) =>
            Fields
                .Where(field => !field.IsValid(values[field.Name]))
                .Select(field => field.ErrorMessage)
                .ToArray();
    }

    private sealed record CsvFieldDefinition(string Name, Func<string, bool>? FormatValidator = null)
    {
        public string ErrorMessage => FormatValidator is null
            ? $"{Name} is required."
            : $"{Name} is missing or has an invalid format.";

        public bool IsValid(string value) =>
            !string.IsNullOrWhiteSpace(value) && (FormatValidator?.Invoke(value) ?? true);
    }
}
