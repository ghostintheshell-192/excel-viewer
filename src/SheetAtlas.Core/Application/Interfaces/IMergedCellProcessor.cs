using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SheetAtlas.Core.Domain.ValueObjects;

namespace SheetAtlas.Core.Application.Interfaces
{
    public interface IMergedCellProcessor
    {
        Dictionary<string, SACellValue> ProcessMergedCells(WorksheetPart worksheetPart, SharedStringTable? sharedStringTable);
    }
}
