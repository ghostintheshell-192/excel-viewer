using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Models.Search;

/// <summary>
/// Represents a sheet occurrence in search results
/// </summary>
public interface ISheetOccurrence : IToggleable
{
    string SheetName { get; }
    IReadOnlyList<ICellOccurrence> CellOccurrences { get; }

    // Property to indicate if the sheet was matched by name
    bool IsMatchedOnSheetName { get; }

    // Reference to parent file
    IFileOccurrence FileOccurrence { get; }

    // Convenience property for accessing file info
    IFileLoadResultViewModel File { get; }

    // Method to add a cell occurrence
    ICellOccurrence AddCellOccurrence(int row, int column, string value, Dictionary<string, string> context);

    // Method to mark the sheet as matched by name
    void SetMatchedOnSheetName();
}
