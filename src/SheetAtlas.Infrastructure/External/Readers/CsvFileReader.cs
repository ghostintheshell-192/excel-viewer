using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Core.Application.Interfaces;
using SheetAtlas.Core.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace SheetAtlas.Infrastructure.External.Readers
{
    /// <summary>
    /// Reader for CSV (Comma-Separated Values) files
    /// </summary>
    /// <remarks>
    /// Uses CsvHelper library with auto-detection for delimiter.
    /// CSV files are converted to ExcelFile with a single sheet named "Data".
    /// Supports configuration via CsvReaderOptions for delimiter, encoding, and culture.
    /// </remarks>
    public class CsvFileReader : IConfigurableFileReader
    {
        private readonly ILogger<CsvFileReader> _logger;
        private CsvReaderOptions _options;

        public CsvFileReader(ILogger<CsvFileReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = CsvReaderOptions.Default;
        }

        public IReadOnlyList<string> SupportedExtensions =>
            new[] { ".csv" }.AsReadOnly();

        public void Configure(IReaderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options is not CsvReaderOptions csvOptions)
                throw new ArgumentException($"Expected CsvReaderOptions but got {options.GetType().Name}", nameof(options));

            _options = csvOptions;
            _logger.LogDebug("Configured CSV reader with delimiter '{Delimiter}', encoding '{Encoding}'",
                _options.Delimiter, _options.Encoding.WebName);
        }

        public async Task<ExcelFile> ReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var errors = new List<ExcelError>();
            var sheets = new Dictionary<string, DataTable>();

            // Validation: Fail fast for invalid input
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                return await Task.Run(() =>
                {
                    // Auto-detect delimiter if using default comma
                    char delimiter = _options.Delimiter;
                    if (_options == CsvReaderOptions.Default)
                    {
                        delimiter = DetectDelimiter(filePath);
                        _logger.LogDebug("Auto-detected delimiter: '{Delimiter}'", delimiter);
                    }

                    var config = new CsvConfiguration(_options.Culture)
                    {
                        Delimiter = delimiter.ToString(),
                        HasHeaderRecord = _options.HasHeaderRow,
                        Encoding = _options.Encoding,
                        BadDataFound = null, // Ignore malformed rows instead of throwing
                        MissingFieldFound = null, // Ignore missing fields
                        TrimOptions = TrimOptions.Trim,
                        DetectDelimiter = false // We handle detection manually
                    };

                    using var reader = new StreamReader(filePath, _options.Encoding);
                    using var csv = new CsvReader(reader, config);

                    // Read all records as dynamic objects
                    var records = new List<dynamic>();

                    try
                    {
                        records = csv.GetRecords<dynamic>().ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading CSV records from {FilePath}", filePath);
                        errors.Add(ExcelError.Critical("File", $"Error parsing CSV: {ex.Message}", ex));
                        return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
                    }

                    if (records.Count == 0)
                    {
                        _logger.LogWarning("CSV file {FilePath} contains no data", filePath);
                        errors.Add(ExcelError.Warning("File", "CSV file contains no data rows"));
                    }

                    // Convert to DataTable
                    var dataTable = ConvertToDataTable(Path.GetFileNameWithoutExtension(filePath), records);
                    sheets["Data"] = dataTable;

                    _logger.LogInformation("Read CSV file with {RowCount} rows and {ColumnCount} columns",
                        dataTable.Rows.Count, dataTable.Columns.Count);

                    var status = DetermineLoadStatus(sheets, errors);
                    return new ExcelFile(filePath, status, sheets, errors);
                }, cancellationToken);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Null filepath passed to ReadAsync");
                throw; // Programming bug - rethrow
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("File read cancelled: {Path}", filePath);
                throw; // Propagate cancellation
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O error reading CSV file: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Cannot access file: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied reading CSV file: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Access denied: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reading CSV file: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Error reading file: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
        }

        private DataTable ConvertToDataTable(string fileName, List<dynamic> records)
        {
            var tableName = $"{fileName.Replace(' ', '_').Replace('-', '_')}_Data";
            var dataTable = new DataTable(tableName);

            if (records.Count == 0)
            {
                return dataTable;
            }

            // Get column names from first record
            var firstRecord = records[0] as IDictionary<string, object>;
            if (firstRecord == null)
            {
                _logger.LogWarning("Unable to extract column names from CSV record");
                return dataTable;
            }

            var columnNameCounts = new Dictionary<string, int>();

            // Create columns
            foreach (var kvp in firstRecord)
            {
                string columnName = kvp.Key;
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    columnName = $"Column_{dataTable.Columns.Count}";
                }

                string uniqueColumnName = EnsureUniqueColumnName(columnName, columnNameCounts);
                dataTable.Columns.Add(uniqueColumnName, typeof(string));
            }

            // Add data rows
            foreach (var record in records)
            {
                var recordDict = record as IDictionary<string, object>;
                if (recordDict == null) continue;

                var row = dataTable.NewRow();
                int columnIndex = 0;

                foreach (var kvp in recordDict)
                {
                    if (columnIndex < dataTable.Columns.Count)
                    {
                        row[columnIndex] = kvp.Value?.ToString() ?? string.Empty;
                        columnIndex++;
                    }
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private char DetectDelimiter(string filePath)
        {
            // Read first few lines to detect delimiter
            var delimiters = new[] { ',', ';', '\t', '|' };
            var delimiterCounts = new Dictionary<char, int>();

            try
            {
                using var reader = new StreamReader(filePath, _options.Encoding);

                // Read first 5 lines for analysis
                var linesToAnalyze = new List<string>();
                for (int i = 0; i < 5 && !reader.EndOfStream; i++)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        linesToAnalyze.Add(line);
                    }
                }

                if (linesToAnalyze.Count == 0)
                {
                    _logger.LogWarning("CSV file is empty, using default delimiter ','");
                    return ',';
                }

                // Count each delimiter occurrence and check consistency
                foreach (var delimiter in delimiters)
                {
                    var counts = linesToAnalyze.Select(line => line.Count(c => c == delimiter)).ToList();

                    // Check if delimiter appears consistently across lines
                    if (counts.All(c => c > 0) && counts.Distinct().Count() == 1)
                    {
                        delimiterCounts[delimiter] = counts[0];
                    }
                }

                // Return delimiter with highest consistent count
                if (delimiterCounts.Any())
                {
                    var bestDelimiter = delimiterCounts.OrderByDescending(kvp => kvp.Value).First().Key;
                    return bestDelimiter;
                }

                // Default to comma if no consistent delimiter found
                _logger.LogDebug("No consistent delimiter found, using default ','");
                return ',';
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error detecting delimiter, using default ','");
                return ',';
            }
        }

        private string EnsureUniqueColumnName(string baseName, Dictionary<string, int> columnNameCounts)
        {
            if (!columnNameCounts.ContainsKey(baseName))
            {
                columnNameCounts[baseName] = 1;
                return baseName;
            }

            columnNameCounts[baseName]++;
            return $"{baseName}_{columnNameCounts[baseName]}";
        }

        private LoadStatus DetermineLoadStatus(Dictionary<string, DataTable> sheets, List<ExcelError> errors)
        {
            var hasErrors = errors.Any(e => e.Level == ErrorLevel.Error || e.Level == ErrorLevel.Critical);

            if (!hasErrors)
                return LoadStatus.Success;

            return sheets.Any() ? LoadStatus.PartialSuccess : LoadStatus.Failed;
        }
    }
}
