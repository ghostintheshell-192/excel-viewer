using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.Core.Domain.ValueObjects;
using ExcelViewer.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ExcelViewer.Infrastructure.External
{
    public interface IExcelReaderService
    {
        Task<ExcelFile> LoadFileAsync(string filePath);
        Task<List<ExcelFile>> LoadFilesAsync(IEnumerable<string> filePaths);
    }

    public class ExcelReaderService : IExcelReaderService
    {
        private readonly ILogger<ExcelReaderService> _logger;
        private readonly ICellReferenceParser _cellParser;
        private readonly IMergedCellProcessor _mergedCellProcessor;

        public ExcelReaderService(ILogger<ExcelReaderService> logger, ICellReferenceParser cellParser, IMergedCellProcessor mergedCellProcessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cellParser = cellParser ?? throw new ArgumentNullException(nameof(cellParser));
            _mergedCellProcessor = mergedCellProcessor ?? throw new ArgumentNullException(nameof(mergedCellProcessor));
        }

        public async Task<List<ExcelFile>> LoadFilesAsync(IEnumerable<string> filePaths)
        {
            var results = new List<ExcelFile>();
            foreach (var filePath in filePaths)
            {
                var file = await LoadFileAsync(filePath);
                results.Add(file);
            }
            return results;
        }

        public async Task<ExcelFile> LoadFileAsync(string filePath)
        {
            var errors = new List<ExcelError>();
            var sheets = new Dictionary<string, DataTable>();

            // Validation: Fail fast for invalid input
            if (string.IsNullOrWhiteSpace(filePath))
            {
                errors.Add(ExcelError.Critical("File", "File path is null or empty"));
                return new ExcelFile(filePath ?? "unknown", LoadStatus.Failed, sheets, errors);
            }

            if (!File.Exists(filePath))
            {
                errors.Add(ExcelError.Critical("File", $"File not found: {filePath}"));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }

            try
            {
                return await Task.Run(() =>
                {
                    using var document = OpenDocument(filePath);
                    var workbookPart = document.WorkbookPart;

                    if (workbookPart == null)
                    {
                        errors.Add(ExcelError.FileError("Workbook part not found in Excel file"));
                        return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
                    }

                    var sheetElements = GetSheets(workbookPart);
                    _logger.LogInformation("Reading Excel file with {SheetCount} sheets", sheetElements.Count());

                    foreach (var sheet in sheetElements)
                    {
                        var sheetName = sheet.Name?.Value;
                        if (string.IsNullOrEmpty(sheetName))
                        {
                            errors.Add(ExcelError.Warning("File", "Found sheet with empty name, skipping"));
                            continue;
                        }

                        try
                        {
                            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                            var dataTable = ProcessSheet(Path.GetFileNameWithoutExtension(filePath), sheetName, workbookPart, worksheetPart);
                            sheets[sheetName] = dataTable;
                            _logger.LogDebug("Sheet {SheetName} read successfully", sheetName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error reading sheet {SheetName}", sheetName);
                            errors.Add(ExcelError.SheetError(sheetName, $"Failed to read sheet: {ex.Message}", ex));
                        }
                    }

                    var status = DetermineLoadStatus(sheets, errors);
                    return new ExcelFile(filePath, status, sheets, errors);
                });
            }
            catch (FileFormatException ex)
            {
                // File format errors: .xls files, corrupted packages, unsupported formats
                _logger.LogError(ex, "Unsupported or corrupted file format: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Unsupported file format (.xls files are not supported, use .xlsx): {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
            catch (IOException ex)
            {
                // File I/O errors: locked, permission denied, network issues
                _logger.LogError(ex, "I/O error reading Excel file: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Cannot access file: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
            catch (InvalidOperationException ex)
            {
                // OpenXml-specific errors: corrupted file structure
                _logger.LogError(ex, "Invalid Excel file format: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Invalid Excel file: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
            catch (OpenXmlPackageException ex)
            {
                // OpenXml package errors: file corrupted or not a valid Excel file
                _logger.LogError(ex, "Excel file is corrupted or invalid: {Path}", filePath);
                errors.Add(ExcelError.Critical("File", $"Corrupted Excel file: {ex.Message}", ex));
                return new ExcelFile(filePath, LoadStatus.Failed, sheets, errors);
            }
        }

        private SpreadsheetDocument OpenDocument(string filePath)
        {
            return SpreadsheetDocument.Open(filePath, false);
        }

        private IEnumerable<Sheet> GetSheets(WorkbookPart workbookPart)
        {
            return workbookPart.Workbook.Descendants<Sheet>();
        }

        private DataTable ProcessSheet(string fileName, string sheetName, WorkbookPart workbookPart, WorksheetPart worksheetPart)
        {
            var tableName = CreateTableName(fileName, sheetName);
            var dataTable = new DataTable(tableName);
            var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

            var mergedCells = _mergedCellProcessor.ProcessMergedCells(worksheetPart, sharedStringTable);
            var headerColumns = ProcessHeaderRow(worksheetPart, sharedStringTable, mergedCells);

            if (!headerColumns.Any())
            {
                _logger.LogWarning("Sheet {SheetName} has no header row", sheetName);
                return dataTable;
            }

            CreateDataTableColumns(dataTable, headerColumns);
            PopulateDataRows(dataTable, worksheetPart, sharedStringTable, mergedCells, headerColumns);

            return dataTable;
        }

        private string CreateTableName(string fileName, string sheetName)
        {
            var safeFileName = fileName.Replace(' ', '_').Replace('-', '_');
            var safeSheetName = sheetName.Replace(' ', '_').Replace('-', '_');
            return $"{safeFileName}_{safeSheetName}";
        }

        private Dictionary<int, string> ProcessHeaderRow(WorksheetPart worksheetPart, SharedStringTable? sharedStringTable, Dictionary<string, string> mergedCells)
        {
            var firstRow = worksheetPart.Worksheet.Descendants<Row>().FirstOrDefault();
            if (firstRow == null)
                return new Dictionary<int, string>();

            var headerValues = new Dictionary<int, string>();
            foreach (var cell in firstRow.Elements<Cell>())
            {
                if (cell.CellReference == null) continue;

                int columnIndex = _cellParser.GetColumnIndex(cell.CellReference.Value);
                string cellValue = GetCellValueWithMerge(cell, sharedStringTable, mergedCells);
                headerValues[columnIndex] = cellValue;
            }

            return headerValues;
        }

        private void CreateDataTableColumns(DataTable dataTable, Dictionary<int, string> headerColumns)
        {
            if (!headerColumns.Any()) return;

            int firstCol = headerColumns.Keys.Min();
            int lastCol = headerColumns.Keys.Max();
            var columnNameCounts = new Dictionary<string, int>();

            for (int i = firstCol; i <= lastCol; i++)
            {
                string headerValue = headerColumns.TryGetValue(i, out var value) && !string.IsNullOrWhiteSpace(value)
                    ? value
                    : $"Column_{i}";

                string uniqueColumnName = EnsureUniqueColumnName(headerValue, columnNameCounts);
                dataTable.Columns.Add(uniqueColumnName);
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

        private void PopulateDataRows(DataTable dataTable, WorksheetPart worksheetPart, SharedStringTable? sharedStringTable, Dictionary<string, string> mergedCells, Dictionary<int, string> headerColumns)
        {
            int firstCol = headerColumns.Keys.Min();
            bool isFirstRow = true;

            foreach (var row in worksheetPart.Worksheet.Descendants<Row>())
            {
                if (isFirstRow)
                {
                    isFirstRow = false;
                    continue; // Skip header row
                }

                var dataRow = CreateDataRow(dataTable, row, sharedStringTable, mergedCells, firstCol);
                if (dataRow != null)
                {
                    dataTable.Rows.Add(dataRow);
                }
            }
        }

        private DataRow? CreateDataRow(DataTable dataTable, Row row, SharedStringTable? sharedStringTable, Dictionary<string, string> mergedCells, int firstCol)
        {
            var dataRow = dataTable.NewRow();
            bool hasData = false;

            foreach (var cell in row.Elements<Cell>())
            {
                if (cell.CellReference == null) continue;

                int columnIndex = _cellParser.GetColumnIndex(cell.CellReference.Value) - firstCol;
                if (columnIndex < 0 || columnIndex >= dataTable.Columns.Count)
                    continue;

                string cellValue = GetCellValueWithMerge(cell, sharedStringTable, mergedCells);
                dataRow[columnIndex] = cellValue;
                hasData = true;
            }

            return hasData ? dataRow : null;
        }

        private string GetCellValueWithMerge(Cell cell, SharedStringTable? sharedStringTable, Dictionary<string, string> mergedCells)
        {
            if (cell.CellReference?.Value != null && mergedCells.TryGetValue(cell.CellReference.Value, out string mergedValue))
            {
                return mergedValue;
            }

            return GetCellValue(cell, sharedStringTable);
        }

        private LoadStatus DetermineLoadStatus(Dictionary<string, DataTable> sheets, List<ExcelError> errors)
        {
            var hasErrors = errors.Any(e => e.Level == ErrorLevel.Error || e.Level == ErrorLevel.Critical);

            if (!hasErrors)
                return LoadStatus.Success;

            return sheets.Any() ? LoadStatus.PartialSuccess : LoadStatus.Failed;
        }

        private string GetCellValue(Cell cell, SharedStringTable? sharedStringTable)
        {
            if (cell == null)
                return string.Empty;

            string value = cell.InnerText;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && sharedStringTable != null)
            {
                if (int.TryParse(value, out int index))
                {
                    value = sharedStringTable.ElementAt(index).InnerText;
                }
            }
            else if (cell.DataType != null && cell.DataType.Value == CellValues.Boolean)
            {
                value = value == "1" ? "TRUE" : "FALSE";
            }

            return value ?? string.Empty;
        }
    }
}
