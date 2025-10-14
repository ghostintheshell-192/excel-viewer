using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Core.Application.Interfaces;
using SheetAtlas.Core.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using SheetAtlas.Logging.Models;

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
            var sheets = new Dictionary<string, SASheetData>();

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

                    // Stream records directly without materializing entire dataset
                    SASheetData sheetData;
                    try
                    {
                        sheetData = ConvertToSASheetDataStreaming(Path.GetFileNameWithoutExtension(filePath), csv);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading CSV records from {FilePath}", filePath);
                        errors.Add(ExcelError.Critical("File", $"Error parsing CSV: {ex.Message}", ex));
                        return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
                    }

                    if (sheetData.RowCount == 0)
                    {
                        _logger.LogWarning("CSV file {FilePath} contains no data", filePath);
                        errors.Add(ExcelError.Warning("File", "CSV file contains no data rows"));
                    }

                    sheets["Data"] = sheetData;

                    _logger.LogInformation("Read CSV file with {RowCount} rows and {ColumnCount} columns",
                        sheetData.RowCount, sheetData.ColumnCount);

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

        private SASheetData ConvertToSASheetDataStreaming(string fileName, CsvReader csv)
        {
            var sheetName = "Data";

            // String pool for deduplicating text values (categories, repeated strings, etc.)
            var stringPool = new StringPool(initialCapacity: 2048);

            // Read records lazily - no ToList()!
            var records = csv.GetRecords<dynamic>();

            SASheetData? sheetData = null;
            var columnNameCounts = new Dictionary<string, int>();
            List<string>? columnNames = null;
            int rowCount = 0;
            int totalStrings = 0;

            foreach (var record in records)
            {
                rowCount++;

                var recordDict = record as IDictionary<string, object>;
                if (recordDict == null)
                {
                    _logger.LogWarning("Skipping non-dictionary record at row {RowNumber}", rowCount);
                    continue;
                }

                // Initialize column names from first record
                if (columnNames == null)
                {
                    columnNames = new List<string>();
                    int colIndex = 0;

                    foreach (var kvp in recordDict)
                    {
                        string columnName = kvp.Key;
                        if (string.IsNullOrWhiteSpace(columnName))
                        {
                            columnName = $"Column_{colIndex}";
                        }

                        string uniqueColumnName = EnsureUniqueColumnName(columnName, columnNameCounts);
                        // Intern column names (often repeated across sheets)
                        columnNames.Add(stringPool.Intern(uniqueColumnName));
                        colIndex++;
                    }

                    sheetData = new SASheetData(sheetName, columnNames.ToArray());
                }

                // Process row data
                var rowData = new SACellData[columnNames.Count];
                int columnIndex = 0;

                foreach (var kvp in recordDict)
                {
                    if (columnIndex < columnNames.Count)
                    {
                        // Use FromString for auto-type detection with string interning
                        string cellText = kvp.Value?.ToString() ?? string.Empty;
                        totalStrings++;
                        rowData[columnIndex] = new SACellData(SACellValue.FromString(cellText, stringPool));
                        columnIndex++;
                    }
                }

                sheetData!.AddRow(rowData);
            }

            // Handle empty file
            if (sheetData == null)
            {
                _logger.LogWarning("CSV file contains no valid records");
                return new SASheetData(sheetName, Array.Empty<string>());
            }

            // Log interning statistics
            var memorySaved = stringPool.EstimatedMemorySaved(totalStrings);
            _logger.LogDebug("String interning: {UniqueStrings} unique from {TotalStrings} total (~{MemorySavedKB} KB saved)",
                stringPool.Count, totalStrings, memorySaved / 1024);

            // Trim excess capacity to save memory
            sheetData.TrimExcess();
            _logger.LogDebug("Sheet trimmed to exact size: {RowCount} rows × {ColumnCount} cols = {CellCount} cells",
                sheetData.RowCount, sheetData.ColumnCount, sheetData.CellCount);

            return sheetData;
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

        private LoadStatus DetermineLoadStatus(Dictionary<string, SASheetData> sheets, List<ExcelError> errors)
        {
            var hasErrors = errors.Any(e => e.Level == LogSeverity.Error || e.Level == LogSeverity.Critical);

            if (!hasErrors)
                return LoadStatus.Success;

            return sheets.Any() ? LoadStatus.PartialSuccess : LoadStatus.Failed;
        }
    }
}
