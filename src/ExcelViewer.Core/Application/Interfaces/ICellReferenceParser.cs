namespace ExcelViewer.Core.Application.Interfaces
{
    public interface ICellReferenceParser
    {
        string GetColumnName(string cellReference);
        int GetColumnIndex(string cellReference);
        int GetRowIndex(string cellReference);
        string CreateCellReference(int columnIndex, int rowIndex);
        string GetColumnNameFromIndex(int columnIndex);
    }
}
