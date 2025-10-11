using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SheetAtlas.Core.Application.Interfaces;
using SheetAtlas.Core.Domain.ValueObjects;

namespace SheetAtlas.Core.Application.Services
{
    public class MergedCellProcessor : IMergedCellProcessor
    {
        private readonly ICellReferenceParser _cellParser;
        private readonly ICellValueReader _cellValueReader;

        public MergedCellProcessor(ICellReferenceParser cellParser, ICellValueReader cellValueReader)
        {
            _cellParser = cellParser ?? throw new ArgumentNullException(nameof(cellParser));
            _cellValueReader = cellValueReader ?? throw new ArgumentNullException(nameof(cellValueReader));
        }

        public Dictionary<string, SACellValue> ProcessMergedCells(WorksheetPart worksheetPart, SharedStringTable? sharedStringTable)
        {
            var mergedCells = new Dictionary<string, SACellValue>();
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

                var sourceValue = _cellValueReader.GetCellValue(sourceCell, sharedStringTable);
                PopulateMergedRange(mergedCells, range[0], range[1], sourceValue);
            }

            return mergedCells;
        }

        private void PopulateMergedRange(Dictionary<string, SACellValue> mergedCells, string startCellRef, string endCellRef, SACellValue value)
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

    }
}
