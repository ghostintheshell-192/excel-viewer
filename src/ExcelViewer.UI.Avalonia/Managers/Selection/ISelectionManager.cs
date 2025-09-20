using ExcelViewer.UI.Avalonia.Models.Search;
using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Managers.Selection;

/// <summary>
/// Manager for selection and visibility operations
/// </summary>
public interface ISelectionManager
{
    // Selected items collections
    IReadOnlyList<ICellOccurrence> SelectedCells { get; }
    IReadOnlyList<ISheetOccurrence> SelectedSheets { get; }

    // Selection methods
    void ToggleCellSelection(ICellOccurrence cell);
    void ToggleSheetSelection(ISheetOccurrence sheet);
    void ClearSelections();

    // Visibility methods
    void ToggleFileVisibility(IFileLoadResultViewModel file);
    void SetFileVisibility(IFileLoadResultViewModel? file = null, bool? visible = null);
    void ShowAllFiles();
    void ShowOnlyFile(IFileLoadResultViewModel file);

    // Method to update visibility statistics
    void UpdateVisibilityStats();

    // Method to update grouped results reference
    void UpdateGroupedResults(IEnumerable<IGroupedSearchResult> results);

    // Events for change notifications
    event EventHandler<EventArgs>? SelectionChanged;
    event EventHandler<EventArgs>? VisibilityChanged;
}