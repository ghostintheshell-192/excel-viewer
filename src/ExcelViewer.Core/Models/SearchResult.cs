
namespace ExcelViewer.Core.Models
{
    /// <summary>
    /// Represents a search result within an Excel file
    /// </summary>
    public class SearchOptions
    {
        public bool CaseSensitive { get; set; }
        public bool ExactMatch { get; set; }
        public bool UseRegex { get; set; }
    }

    public class SearchResult
    {
        public ExcelFile SourceFile { get; set; }
        public string SheetName { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string Value { get; set; }
        public Dictionary<string, string> Context { get; set; }

        // Calculated properties for compatibility
        public string FileName => SourceFile?.FileName ?? string.Empty;
        public string CellAddress => $"{GetColumnName(Column)}{Row + 1}";

        public SearchResult()
        {
            Context = new Dictionary<string, string>();
        }

        public SearchResult(ExcelFile sourceFile, string sheetName, int row, int column, string value)
            : this()
        {
            SourceFile = sourceFile;
            SheetName = sheetName;
            Row = row;
            Column = column;
            Value = value;
        }

        private static string GetColumnName(int columnIndex)
        {
            string columnName = "";
            while (columnIndex >= 0)
            {
                columnName = (char)('A' + (columnIndex % 26)) + columnName;
                columnIndex = (columnIndex / 26) - 1;
            }
            return columnName;
        }
    }
}
