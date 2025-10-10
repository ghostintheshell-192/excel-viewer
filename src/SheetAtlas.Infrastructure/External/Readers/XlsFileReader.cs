using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;
using ExcelDataReader;

namespace SheetAtlas.Infrastructure.External.Readers
{
    /// <summary>
    /// Reader for legacy Excel binary formats (.xls, .xlt)
    /// </summary>
    /// <remarks>
    /// Uses ExcelDataReader library for BIFF8 format support.
    /// Limitations: Does not support merged cell detection or formula extraction.
    /// </remarks>
    public class XlsFileReader : IFileFormatReader
    {
        private readonly ILogger<XlsFileReader> _logger;
        private static bool _encodingProviderRegistered = false;
        private static readonly object _encodingLock = new object();

        public XlsFileReader(ILogger<XlsFileReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RegisterEncodingProvider();
        }

        public IReadOnlyList<string> SupportedExtensions =>
            new[] { ".xls", ".xlt" }.AsReadOnly();

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
                    using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    // ExcelDataReader can auto-detect format, but we specify .xls explicitly
                    using var reader = ExcelReaderFactory.CreateBinaryReader(stream);

                    if (reader == null)
                    {
                        errors.Add(ExcelError.Critical("File", "Failed to create Excel reader for .xls file"));
                        return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
                    }

                    // Read all sheets into a DataSet
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration
                        {
                            // We'll handle headers manually to match OpenXML behavior
                            UseHeaderRow = false
                        }
                    });

                    if (dataSet == null || dataSet.Tables.Count == 0)
                    {
                        _logger.LogWarning("File {FilePath} contains no sheets", filePath);
                        errors.Add(ExcelError.Warning("File", "File contains no data sheets"));
                        return new ExcelFile(filePath, LoadStatus.Success, sheets, errors);
                    }

                    _logger.LogInformation("Reading .xls file with {SheetCount} sheets", dataSet.Tables.Count);

                    // Convert each DataTable to our format
                    foreach (DataTable table in dataSet.Tables)
                    {
                        // Check cancellation before processing each sheet
                        cancellationToken.ThrowIfCancellationRequested();

                        var sheetName = table.TableName;
                        if (string.IsNullOrEmpty(sheetName))
                        {
                            errors.Add(ExcelError.Warning("File", "Found sheet with empty name, skipping"));
                            continue;
                        }

                        try
                        {
                            var processedTable = ProcessSheet(Path.GetFileNameWithoutExtension(filePath), table);
                            sheets[sheetName] = processedTable;
                            _logger.LogDebug("Sheet {SheetName} read successfully with {RowCount} rows",
                                sheetName, processedTable.Rows.Count);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing sheet {SheetName}", sheetName);
                            errors.Add(ExcelError.SheetError(sheetName, $"Error reading sheet: {ex.Message}", ex));
                        }
                    }

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
                _logger.LogError(ex, "I/O error reading .xls file: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Cannot access file: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied reading .xls file: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Access denied: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
            catch (Exception ex)
            {
                // Catch-all for unexpected errors (includes ExcelDataReader errors)
                _logger.LogError(ex, "Error reading .xls file: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Error reading file: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
        }

        private DataTable ProcessSheet(string fileName, DataTable sourceTable)
        {
            var tableName = CreateTableName(fileName, sourceTable.TableName);
            var resultTable = new DataTable(tableName);

            if (sourceTable.Rows.Count == 0)
            {
                _logger.LogWarning("Sheet {SheetName} is empty", sourceTable.TableName);
                return resultTable;
            }

            // First row is treated as header (matching OpenXML behavior)
            var headerRow = sourceTable.Rows[0];
            var columnNameCounts = new Dictionary<string, int>();

            // Create columns from first row
            for (int i = 0; i < sourceTable.Columns.Count; i++)
            {
                string headerValue = headerRow[i]?.ToString()?.Trim() ?? string.Empty;

                // Use "Column_N" if header is empty or whitespace
                if (string.IsNullOrWhiteSpace(headerValue))
                {
                    headerValue = $"Column_{i}";
                }

                string uniqueColumnName = EnsureUniqueColumnName(headerValue, columnNameCounts);
                resultTable.Columns.Add(uniqueColumnName, typeof(string));
            }

            // Add data rows (skip first row which is header)
            for (int rowIndex = 1; rowIndex < sourceTable.Rows.Count; rowIndex++)
            {
                var sourceRow = sourceTable.Rows[rowIndex];
                var newRow = resultTable.NewRow();
                bool hasData = false;

                for (int colIndex = 0; colIndex < resultTable.Columns.Count && colIndex < sourceTable.Columns.Count; colIndex++)
                {
                    var cellValue = sourceRow[colIndex]?.ToString() ?? string.Empty;
                    newRow[colIndex] = cellValue;

                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        hasData = true;
                    }
                }

                // Only add row if it contains at least some data
                if (hasData)
                {
                    resultTable.Rows.Add(newRow);
                }
            }

            return resultTable;
        }

        private string CreateTableName(string fileName, string sheetName)
        {
            var safeFileName = fileName.Replace(' ', '_').Replace('-', '_');
            var safeSheetName = sheetName.Replace(' ', '_').Replace('-', '_');
            return $"{safeFileName}_{safeSheetName}";
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

        private void RegisterEncodingProvider()
        {
            // ExcelDataReader requires this for legacy encodings in .xls files
            // Only register once per application lifetime (thread-safe)
            if (!_encodingProviderRegistered)
            {
                lock (_encodingLock)
                {
                    if (!_encodingProviderRegistered)
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        _encodingProviderRegistered = true;
                        _logger.LogDebug("Registered CodePagesEncodingProvider for legacy .xls encoding support");
                    }
                }
            }
        }
    }
}
