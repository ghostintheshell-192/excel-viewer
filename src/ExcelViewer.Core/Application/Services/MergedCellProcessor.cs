using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelViewer.Core.Application.Interfaces;

namespace ExcelViewer.Core.Application.Services
{
    public class MergedCellProcessor : IMergedCellProcessor
    {
        private readonly ICellReferenceParser _cellParser;

        public MergedCellProcessor(ICellReferenceParser cellParser)
        {
            _cellParser = cellParser ?? throw new ArgumentNullException(nameof(cellParser));
        }

        public Dictionary<string, string> ProcessMergedCells(WorksheetPart worksheetPart, SharedStringTable? sharedStringTable)
        {
            var mergedCells = new Dictionary<string, string>();
            var cellsInSheet = worksheetPart.Worksheet.Descendants<Cell>();

            var mergeCellsElement = worksheetPart.Worksheet.Elements<MergeCells>().FirstOrDefault();
            if (mergeCellsElement == null)
                return mergedCells;

            foreach (var mergeCell in mergeCellsElement.Elements<MergeCell>())
            {
                if (mergeCell.Reference?.Value == null)
                    continue;

                var cellReference = mergeCell.Reference.Value;
                var range = cellReference.Split(':');
                if (range.Length != 2)
                    continue;

                var startCell = range[0];
                var sourceCell = cellsInSheet.FirstOrDefault(c => c.CellReference?.Value == startCell);
                if (sourceCell == null)
                    continue;

                var sourceValue = GetCellValue(sourceCell, sharedStringTable);
                PopulateMergedRange(mergedCells, range[0], range[1], sourceValue);
            }

            return mergedCells;
        }

        private void PopulateMergedRange(Dictionary<string, string> mergedCells, string startCellRef, string endCellRef, string value)
        {
            var startColIndex = _cellParser.GetColumnIndex(startCellRef);
            var endColIndex = _cellParser.GetColumnIndex(endCellRef);
            var startRowIndex = _cellParser.GetRowIndex(startCellRef);
            var endRowIndex = _cellParser.GetRowIndex(endCellRef);

            for (int row = startRowIndex; row <= endRowIndex; row++)
            {
                for (int col = startColIndex; col <= endColIndex; col++)
                {
                    var cellRef = _cellParser.CreateCellReference(col, row);
                    mergedCells[cellRef] = value;
                }
            }
        }

        private string GetCellValue(Cell cell, SharedStringTable? sharedStringTable)
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