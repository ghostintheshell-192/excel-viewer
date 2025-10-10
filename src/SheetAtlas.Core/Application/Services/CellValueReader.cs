using System.Collections.Concurrent;
using DocumentFormat.OpenXml.Spreadsheet;
using SheetAtlas.Core.Application.Interfaces;

namespace SheetAtlas.Core.Application.Services
{
    /// <summary>
    /// Reads and parses cell values from Excel worksheets.
    /// Handles different cell data types: shared strings, booleans, numbers, and formulas.
    /// Uses string interning to reduce memory footprint for duplicate values.
    /// </summary>
    public class CellValueReader : ICellValueReader
    {
        private readonly ConcurrentDictionary<string, string> _stringPool = new();
        private const int MaxPoolSize = 50000; // Limit pool size to prevent unbounded growth
        private const int MaxInternLength = 100; // Only intern short strings (likely to be duplicates)

        public string GetCellValue(Cell cell, SharedStringTable? sharedStringTable)
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

            value = value ?? string.Empty;

            // Intern short strings to reduce memory duplication
            // Only intern if pool hasn't exceeded max size to prevent unbounded growth
            if (value.Length > 0 && value.Length <= MaxInternLength && _stringPool.Count < MaxPoolSize)
            {
                value = _stringPool.GetOrAdd(value, value);
            }

            return value;
        }
    }
}
