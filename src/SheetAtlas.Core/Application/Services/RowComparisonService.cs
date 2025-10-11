using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.Exceptions;
using SheetAtlas.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SheetAtlas.Core.Application.Services
{
    public class RowComparisonService : IRowComparisonService
    {
        private readonly ILogger<RowComparisonService> _logger;

        public RowComparisonService(ILogger<RowComparisonService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public RowComparison CreateRowComparison(RowComparisonRequest request)
        {
            if (request?.SelectedMatches == null || request.SelectedMatches.Count < 2)
                throw new ArgumentException("At least two search results are required for comparison", nameof(request));

            _logger.LogInformation("Creating row comparison from {Count} search results", request.SelectedMatches.Count);

            var excelRows = new List<ExcelRow>();

            foreach (var searchResult in request.SelectedMatches)
            {
                try
                {
                    var excelRow = ExtractRowFromSearchResult(searchResult);
                    excelRows.Add(excelRow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to extract row from search result: {FileName}, Sheet: {SheetName}, Row: {Row}",
                        searchResult.FileName, searchResult.SheetName, searchResult.Row);
                    throw;
                }
            }

            return new RowComparison(excelRows.AsReadOnly(), request.Name);
        }

        public ExcelRow ExtractRowFromSearchResult(SearchResult searchResult)
        {
            if (searchResult?.SourceFile == null)
                throw new ArgumentNullException(nameof(searchResult));

            // Skip non-cell results (filename, sheet name matches)
            if (searchResult.Row < 0 || searchResult.Column < 0)
                throw new ArgumentException("Search result does not represent a valid cell", nameof(searchResult));

            var sheet = searchResult.SourceFile.GetSheet(searchResult.SheetName);
            if (sheet == null)
                throw ComparisonException.MissingSheet(searchResult.SheetName, searchResult.FileName);

            if (searchResult.Row >= sheet.RowCount)
                throw new ArgumentOutOfRangeException(nameof(searchResult), $"Row index {searchResult.Row} is out of range for sheet '{searchResult.SheetName}'");

            // Extract complete row data
            var rowCells = sheet.GetRow(searchResult.Row);
            // Convert SACellData[] to object[] for compatibility with ExcelRow
            var cells = rowCells.Select(cell => (object?)cell.Value.ToString()).ToArray();

            // Get column headers
            var columnHeaders = GetColumnHeaders(searchResult.SourceFile, searchResult.SheetName);

            return new ExcelRow(
                searchResult.SourceFile,
                searchResult.SheetName,
                searchResult.Row,
                cells,
                columnHeaders);
        }

        public IReadOnlyList<string> GetColumnHeaders(ExcelFile file, string sheetName)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var sheet = file.GetSheet(sheetName);
            if (sheet == null)
                throw ComparisonException.MissingSheet(sheetName, file.FilePath);

            // SASheetData already has ColumnNames array
            return sheet.ColumnNames;
        }
    }
}
