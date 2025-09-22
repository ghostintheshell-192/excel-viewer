using System.Data;

namespace ExcelViewer.Core.Domain.Entities
{
    /// <summary>
    /// Represents a complete row from an Excel sheet for comparison purposes
    /// </summary>
    public class ExcelRow
    {
        public ExcelFile SourceFile { get; }
        public string SheetName { get; }
        public int RowIndex { get; }
        public IReadOnlyList<object?> Cells { get; }
        public IReadOnlyList<string> ColumnHeaders { get; }

        // Calculated properties
        public string FileName => SourceFile?.FileName ?? string.Empty;
        public string DisplayName => $"{FileName} - {SheetName} - Row {RowIndex + 1}";

        public ExcelRow(ExcelFile sourceFile, string sheetName, int rowIndex,
                       IReadOnlyList<object?> cells, IReadOnlyList<string> columnHeaders)
        {
            SourceFile = sourceFile ?? throw new ArgumentNullException(nameof(sourceFile));
            SheetName = sheetName ?? throw new ArgumentNullException(nameof(sheetName));
            RowIndex = rowIndex;
            Cells = cells ?? throw new ArgumentNullException(nameof(cells));
            ColumnHeaders = columnHeaders ?? throw new ArgumentNullException(nameof(columnHeaders));
        }

        /// <summary>
        /// Get cell value by column index
        /// </summary>
        public object? GetCell(int columnIndex)
        {
            return columnIndex >= 0 && columnIndex < Cells.Count ? Cells[columnIndex] : null;
        }

        /// <summary>
        /// Get cell value as string
        /// </summary>
        public string GetCellAsString(int columnIndex)
        {
            return GetCell(columnIndex)?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Represents a comparison between multiple Excel rows
    /// </summary>
    public class RowComparison
    {
        public Guid Id { get; }
        public IReadOnlyList<ExcelRow> Rows { get; }
        public DateTime CreatedAt { get; }
        public string Name { get; set; }

        public RowComparison(IReadOnlyList<ExcelRow> rows, string? name = null)
        {
            if (rows == null || rows.Count < 2)
                throw new ArgumentException("At least two rows are required for comparison", nameof(rows));

            Id = Guid.NewGuid();
            Rows = rows;
            CreatedAt = DateTime.UtcNow;
            Name = name ?? $"Comparison {CreatedAt:HH:mm:ss}";
        }

        /// <summary>
        /// Get all unique column headers from all rows
        /// </summary>
        public IReadOnlyList<string> GetAllColumnHeaders()
        {
            return Rows
                .SelectMany(r => r.ColumnHeaders)
                .Distinct()
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Get maximum number of columns across all rows
        /// </summary>
        public int MaxColumns => Rows.Max(r => r.Cells.Count);
    }

    /// <summary>
    /// Represents a request to create a row comparison from search results
    /// </summary>
    public class RowComparisonRequest
    {
        public IReadOnlyList<SearchResult> SelectedMatches { get; }
        public string? Name { get; set; }

        public RowComparisonRequest(IReadOnlyList<SearchResult> selectedMatches, string? name = null)
        {
            SelectedMatches = selectedMatches ?? throw new ArgumentNullException(nameof(selectedMatches));
            Name = name;
        }
    }
}