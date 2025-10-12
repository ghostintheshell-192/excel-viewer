using DocumentFormat.OpenXml.Spreadsheet;
using SheetAtlas.Core.Domain.ValueObjects;

namespace SheetAtlas.Core.Application.Interfaces
{
    /// <summary>
    /// Service responsible for reading and parsing cell values from Excel worksheets.
    /// Handles different cell data types (shared strings, booleans, numbers, dates).
    /// Preserves type information by returning SACellValue instead of string.
    /// </summary>
    public interface ICellValueReader
    {
        /// <summary>
        /// Gets the typed value from an Excel cell, preserving original data type.
        /// </summary>
        /// <param name="cell">The cell to read from</param>
        /// <param name="sharedStringTable">The shared string table for the workbook (if available)</param>
        /// <returns>The cell value with type information, or SACellValue.Empty if cell is null</returns>
        SACellValue GetCellValue(Cell cell, SharedStringTable? sharedStringTable);
    }
}
