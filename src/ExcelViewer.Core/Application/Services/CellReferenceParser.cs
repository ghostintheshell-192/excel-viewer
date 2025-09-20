using System.Text.RegularExpressions;
using ExcelViewer.Core.Application.Interfaces;

namespace ExcelViewer.Core.Application.Services
{
    public class CellReferenceParser : ICellReferenceParser
    {
        private static readonly Regex ColumnRegex = new("[A-Za-z]+", RegexOptions.Compiled);
        private static readonly Regex RowRegex = new("[0-9]+", RegexOptions.Compiled);

        public string GetColumnName(string cellReference)
        {
            var match = ColumnRegex.Match(cellReference);
            return match.Success ? match.Value : string.Empty;
        }

        public int GetColumnIndex(string cellReference)
        {
            string columnName = GetColumnName(cellReference);
            int columnIndex = 0;

            for (int i = 0; i < columnName.Length; i++)
            {
                columnIndex = columnIndex * 26 + (columnName[i] - 'A' + 1);
            }

            return columnIndex - 1;
        }

        public int GetRowIndex(string cellReference)
        {
            var match = RowRegex.Match(cellReference);
            if (int.TryParse(match.Value, out int rowIndex))
            {
                return rowIndex - 1; // Convert to 0-based
            }
            return 0;
        }

        public string CreateCellReference(int columnIndex, int rowIndex)
        {
            string columnName = GetColumnNameFromIndex(columnIndex);
            return $"{columnName}{rowIndex + 1}";
        }

        public string GetColumnNameFromIndex(int columnIndex)
        {
            string columnName = "";
            columnIndex++; // Convert from 0-based to 1-based

            while (columnIndex > 0)
            {
                int modulo = (columnIndex - 1) % 26;
                columnName = (char)('A' + modulo) + columnName;
                columnIndex = (columnIndex - modulo) / 26;
            }

            return columnName;
        }
    }
}