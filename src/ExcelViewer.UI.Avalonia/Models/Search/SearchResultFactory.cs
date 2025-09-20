using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Models.Search;

public class SearchResultFactory : ISearchResultFactory
{
    public IGroupedSearchResult CreateGroupedSearchResult(string value)
    {
        return new GroupedSearchResultImpl(value);
    }

    public IFileOccurrence CreateFileOccurrence(IFileLoadResultViewModel file)
    {
        return new FileOccurrenceImpl(file);
    }

    public ISheetOccurrence CreateSheetOccurrence(IFileOccurrence fileOccurrence, string sheetName)
    {
        return new SheetOccurrenceImpl(fileOccurrence, sheetName);
    }

    public ICellOccurrence CreateCellOccurrence(ISheetOccurrence sheetOccurrence, int row, int column, string value, Dictionary<string, string> context)
    {
        return new CellOccurrenceImpl(sheetOccurrence, row, column, value, context);
    }
}