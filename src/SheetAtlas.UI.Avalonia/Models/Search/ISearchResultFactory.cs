using SheetAtlas.UI.Avalonia.ViewModels;

namespace SheetAtlas.UI.Avalonia.Models.Search;

/// <summary>
/// Factory for creating search result models
/// </summary>
public interface ISearchResultFactory
{
    IGroupedSearchResult CreateGroupedSearchResult(string value);
    IFileOccurrence CreateFileOccurrence(IFileLoadResultViewModel file);
    ISheetOccurrence CreateSheetOccurrence(IFileOccurrence fileOccurrence, string sheetName);
    ICellOccurrence CreateCellOccurrence(ISheetOccurrence sheetOccurrence, int row, int column, string value, Dictionary<string, string> context);
}
