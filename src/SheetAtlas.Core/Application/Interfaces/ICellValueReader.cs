using DocumentFormat.OpenXml.Spreadsheet;

namespace SheetAtlas.Core.Application.Interfaces
{
    /// <summary>
    /// Service responsible for reading and parsing cell values from Excel worksheets.
    /// Handles different cell data types (shared strings, booleans, numbers).
    /// </summary>
    public interface ICellValueReader
    {
        /// <summary>
        /// Gets the text value from an Excel cell, handling different data types appropriately.
        /// </summary>
        /// <param name="cell">The cell to read from</param>
        /// <param name="sharedStringTable">The shared string table for the workbook (if available)</param>
        /// <returns>The cell value as a string, or empty string if cell is null</returns>
        string GetCellValue(Cell cell, SharedStringTable? sharedStringTable);
    }
}
