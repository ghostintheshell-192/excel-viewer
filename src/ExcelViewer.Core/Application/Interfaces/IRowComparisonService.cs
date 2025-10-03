using ExcelViewer.Core.Domain.Entities;

namespace ExcelViewer.Core.Application.Interfaces
{
    public interface IRowComparisonService
    {
        /// <summary>
        /// Create a row comparison from selected search results
        /// </summary>
        Task<RowComparison> CreateRowComparisonAsync(RowComparisonRequest request);

        /// <summary>
        /// Extract complete row data from a search result
        /// </summary>
        Task<ExcelRow> ExtractRowFromSearchResultAsync(SearchResult searchResult);

        /// <summary>
        /// Get column headers for a specific sheet
        /// </summary>
        Task<IReadOnlyList<string>> GetColumnHeadersAsync(ExcelFile file, string sheetName);
    }
}
