using System.Globalization;
using System.Text;

namespace Nodal.FraudLab.Web.Application.Import;

public sealed class CsvImportPreviewService : ICsvImportPreviewService
{
    private static readonly IReadOnlyList<CsvSchemaDefinition> Schemas =
    [
        CreateSchema(
            FraudDataSetKind.CustomerProfile,
            "Customer profile",
            "customers.csv",
            [
                Property("Id", "string", "Unique customer identifier"),
                Property("DisplayName", "string", "Non-empty display name"),
                Property("OnboardedOn", "DateOnly", "ISO date: yyyy-MM-dd", IsDateOnly),
                Property("CountryCode", "string", "Two-letter country code", IsCountryCode),
            ]),
        CreateSchema(
            FraudDataSetKind.FinancialAccount,
            "Financial account",
            "accounts.csv",
            [
                Property("Id", "string", "Unique account identifier"),
                Property("CustomerId", "string", "Customer identifier"),
                Property("AccountType", "string", "Non-empty account type"),
                Property("OpenedOn", "DateOnly", "ISO date: yyyy-MM-dd", IsDateOnly),
                Property("CurrencyCode", "string", "Three-letter currency code", IsCurrencyCode),
            ]),
        CreateSchema(
            FraudDataSetKind.Merchant,
            "Merchant",
            "merchants.csv",
            [
                Property("Id", "string", "Unique merchant identifier"),
                Property("Name", "string", "Non-empty merchant name"),
                Property("MerchantCategoryCode", "string", "Non-empty merchant category code"),
                Property("CountryCode", "string", "Two-letter country code", IsCountryCode),
            ]),
        CreateSchema(
            FraudDataSetKind.DeviceFingerprint,
            "Device fingerprint",
            "devices.csv",
            [
                Property("Id", "string", "Unique device identifier"),
                Property("DeviceType", "string", "Non-empty device type"),
                Property("OperatingSystem", "string", "Non-empty operating system"),
                Property("TrustLevel", "string", "Non-empty trust level"),
            ]),
        CreateSchema(
            FraudDataSetKind.NetworkAddress,
            "Network address",
            "network-addresses.csv",
            [
                Property("Id", "string", "Unique network identifier"),
                Property("CountryCode", "string", "Two-letter country code", IsCountryCode),
                Property("NetworkType", "string", "Non-empty network type"),
            ]),
        CreateSchema(
            FraudDataSetKind.FinancialTransaction,
            "Financial transaction",
            "transactions.csv",
            [
                Property("Id", "string", "Unique transaction identifier"),
                Property("AccountId", "string", "Account identifier"),
                Property("MerchantId", "string", "Merchant identifier"),
                Property("Amount", "decimal", "Positive invariant decimal, for example 1250.00", IsPositiveDecimal),
                Property("CurrencyCode", "string", "Three-letter currency code", IsCurrencyCode),
                Property("OccurredAt", "DateTimeOffset", "ISO 8601 timestamp with offset", IsDateTimeOffset),
                Property("Status", "string", "Non-empty transaction status"),
            ]),
    ];

    public IReadOnlyList<CsvModelDefinition> SupportedModels => Schemas.Select(schema => schema.Model).ToArray();

    public async Task<CsvDataSetPreview> PreviewAsync(
        Stream content,
        string fileName,
        FraudDataSetKind expectedDataSet,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var schema = Schemas.SingleOrDefault(candidate => candidate.Model.DataSet == expectedDataSet)
            ?? throw new ArgumentOutOfRangeException(nameof(expectedDataSet), "Select a supported FraudLab model.");

        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return CreateInvalidPreview(fileName, schema, [], "The CSV file does not contain a header row.");
        }

        var columns = ParseLine(headerLine).Select(column => column.Trim()).ToArray();
        var headerIssues = schema.ValidateHeader(columns);
        if (headerIssues.Count > 0)
        {
            return new CsvDataSetPreview(fileName, schema.Model.DataSet, 0, 0, columns, [], headerIssues.Select(message => new CsvImportIssue(null, message)).ToArray());
        }

        var previewRows = new List<IReadOnlyDictionary<string, string>>();
        var issues = new List<CsvImportIssue>();
        var identifiers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
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

            var rowIssues = schema.ValidateRow(values).ToList();
            var identifier = values["Id"];
            if (identifiers.TryGetValue(identifier, out var originalRow))
            {
                rowIssues.Add($"Id '{identifier}' duplicates row {originalRow}.");
            }
            else if (!string.IsNullOrWhiteSpace(identifier))
            {
                identifiers.Add(identifier, rowNumber);
            }

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

        return new CsvDataSetPreview(fileName, schema.Model.DataSet, totalRows, validRows, columns, previewRows, issues);
    }

    private static CsvSchemaDefinition CreateSchema(
        FraudDataSetKind dataSet,
        string displayName,
        string demoFileName,
        IReadOnlyList<CsvFieldDefinition> fields) =>
        new(
            new CsvModelDefinition(
                dataSet,
                displayName,
                demoFileName,
                fields.Select(field => new CsvModelPropertyDefinition(field.Name, field.DotNetType, true, field.ValidationRule)).ToArray()),
            fields);

    private static CsvFieldDefinition Property(
        string name,
        string dotNetType,
        string validationRule,
        Func<string, bool>? formatValidator = null) =>
        new(name, dotNetType, validationRule, formatValidator);

    private static CsvDataSetPreview CreateInvalidPreview(
        string fileName,
        CsvSchemaDefinition schema,
        IReadOnlyList<string> columns,
        string message) =>
        new(fileName, schema.Model.DataSet, 0, 0, columns, [], [new CsvImportIssue(null, message)]);

    private static bool IsDateOnly(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private static bool IsPositiveDecimal(string value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) && amount > 0;

    private static bool IsDateTimeOffset(string value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _);

    private static bool IsCountryCode(string value) =>
        value.Length == 2 && value.All(char.IsLetter);

    private static bool IsCurrencyCode(string value) =>
        value.Length == 3 && value.All(char.IsLetter);

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

    private sealed record CsvSchemaDefinition(CsvModelDefinition Model, IReadOnlyList<CsvFieldDefinition> Fields)
    {
        public IReadOnlyList<string> ValidateHeader(IReadOnlyList<string> columns)
        {
            var issues = new List<string>();
            var expectedColumns = Fields.Select(field => field.Name).ToArray();
            if (columns.Distinct(StringComparer.OrdinalIgnoreCase).Count() != columns.Count)
            {
                issues.Add("The CSV header contains duplicate column names.");
            }

            var missingColumns = expectedColumns.Where(expected => !columns.Contains(expected, StringComparer.OrdinalIgnoreCase)).ToArray();
            if (missingColumns.Length > 0)
            {
                issues.Add($"Missing required column(s): {string.Join(", ", missingColumns)}.");
            }

            var unexpectedColumns = columns.Where(column => !expectedColumns.Contains(column, StringComparer.OrdinalIgnoreCase)).ToArray();
            if (unexpectedColumns.Length > 0)
            {
                issues.Add($"Unexpected column(s) for {Model.DisplayName}: {string.Join(", ", unexpectedColumns)}.");
            }

            return issues;
        }

        public IReadOnlyList<string> ValidateRow(IReadOnlyDictionary<string, string> values) =>
            Fields
                .Where(field => !field.IsValid(values[field.Name]))
                .Select(field => $"{field.Name} is missing or invalid. Rule: {field.ValidationRule}.")
                .ToArray();
    }

    private sealed record CsvFieldDefinition(
        string Name,
        string DotNetType,
        string ValidationRule,
        Func<string, bool>? FormatValidator = null)
    {
        public bool IsValid(string value) =>
            !string.IsNullOrWhiteSpace(value) && (FormatValidator?.Invoke(value) ?? true);
    }
}
