using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ExcelViewer.Core.Services
{
    public interface IMergedCellProcessor
    {
        Dictionary<string, string> ProcessMergedCells(WorksheetPart worksheetPart, SharedStringTable? sharedStringTable);
    }
}