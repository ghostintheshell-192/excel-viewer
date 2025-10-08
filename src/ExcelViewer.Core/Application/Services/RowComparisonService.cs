using System.Data;
using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.Core.Domain.Exceptions;
using ExcelViewer.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.Core.Application.Services
{
    public class RowComparisonService : IRowComparisonService
    {
        private readonly ILogger<RowComparisonService> _logger;

        public RowComparisonService(ILogger<RowComparisonService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RowComparison> CreateRowComparisonAsync(RowComparisonRequest request)
        {
            if (request?.SelectedMatches == null || request.SelectedMatches.Count < 2)
                throw new ArgumentException("At least two search results are required for comparison", nameof(request));

            _logger.LogInformation("Creating row comparison from {Count} search results", request.SelectedMatches.Count);

            var excelRows = new List<ExcelRow>();

            foreach (var searchResult in request.SelectedMatches)
            {
                try
                {
                    var excelRow = await ExtractRowFromSearchResultAsync(searchResult);
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

        public async Task<ExcelRow> ExtractRowFromSearchResultAsync(SearchResult searchResult)
        {
            if (searchResult?.SourceFile == null)
                throw new ArgumentNullException(nameof(searchResult));

            // Skip non-cell results (filename, sheet name matches)
            if (searchResult.Row < 0 || searchResult.Column < 0)
                throw new ArgumentException("Search result does not represent a valid cell", nameof(searchResult));

            var sheet = searchResult.SourceFile.GetSheet(searchResult.SheetName);
            if (sheet == null)
                throw ComparisonException.MissingSheet(searchResult.SheetName, searchResult.FileName);

            if (searchResult.Row >= sheet.Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(searchResult), $"Row index {searchResult.Row} is out of range for sheet '{searchResult.SheetName}'");

            // Extract complete row data
            var dataRow = sheet.Rows[searchResult.Row];
            var cells = dataRow.ItemArray.ToList().AsReadOnly();

            // Get column headers
            var columnHeaders = await GetColumnHeadersAsync(searchResult.SourceFile, searchResult.SheetName);

            return await Task.FromResult(new ExcelRow(
                searchResult.SourceFile,
                searchResult.SheetName,
                searchResult.Row,
                cells,
                columnHeaders));
        }

        public async Task<IReadOnlyList<string>> GetColumnHeadersAsync(ExcelFile file, string sheetName)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var sheet = file.GetSheet(sheetName);
            if (sheet == null)
                throw ComparisonException.MissingSheet(sheetName, file.FilePath);

            var headers = new List<string>();

            // Use column names from DataTable if available
            foreach (DataColumn column in sheet.Columns)
            {
                headers.Add(column.ColumnName);
            }

            // If no columns or generic column names, try to get headers from first row
            if (headers.Count == 0 || headers.All(h => h.StartsWith("Column")))
            {
                if (sheet.Rows.Count > 0)
                {
                    var firstRow = sheet.Rows[0];
                    headers.Clear();

                    for (int i = 0; i < firstRow.ItemArray.Length; i++)
                    {
                        var headerValue = firstRow[i]?.ToString();
                        headers.Add(string.IsNullOrWhiteSpace(headerValue) ? $"Column {i + 1}" : headerValue);
                    }
                }
            }

            return await Task.FromResult(headers.AsReadOnly());
        }
    }
}
