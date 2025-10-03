using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Models.Search;

/// <summary>
/// Represents a file occurrence in search results
/// </summary>
public interface IFileOccurrence : IToggleable
{
    IFileLoadResultViewModel File { get; }
    IReadOnlyList<ISheetOccurrence> SheetOccurrences { get; }

    // Property to indicate if the file was matched by name
    bool IsMatchedOnFileName { get; }

    // Method to get or add a sheet occurrence
    ISheetOccurrence GetOrAddSheetOccurrence(string sheetName);

    // Method to mark the file as matched by name
    void SetMatchedOnFileName();
}
