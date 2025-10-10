using DocumentFormat.OpenXml.Spreadsheet;
using SheetAtlas.Core.Application.Interfaces;

namespace SheetAtlas.Core.Application.Services
{
    /// <summary>
    /// Reads and parses cell values from Excel worksheets.
    /// Handles different cell data types: shared strings, booleans, numbers, and formulas.
    /// </summary>
    public class CellValueReader : ICellValueReader
    {
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

            return value ?? string.Empty;
        }
    }
}
