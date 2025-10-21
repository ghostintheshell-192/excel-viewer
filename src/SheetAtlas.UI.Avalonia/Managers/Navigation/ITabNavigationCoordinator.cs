using System.ComponentModel;

namespace SheetAtlas.UI.Avalonia.Managers.Navigation;

/// <summary>
/// Coordinates tab visibility and navigation in the main window.
/// Handles showing, hiding, and switching between different tabs (FileDetails, Search, Comparison).
/// </summary>
public interface ITabNavigationCoordinator : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets whether the File Details tab is visible.
    /// </summary>
    bool IsFileDetailsTabVisible { get; set; }

    /// <summary>
    /// Gets or sets whether the Search tab is visible.
    /// </summary>
    bool IsSearchTabVisible { get; set; }

    /// <summary>
    /// Gets or sets whether the Comparison tab is visible.
    /// </summary>
    bool IsComparisonTabVisible { get; set; }

    /// <summary>
    /// Gets or sets the index of the currently selected tab.
    /// -1 indicates no tab is selected (welcome screen).
    /// </summary>
    int SelectedTabIndex { get; set; }

    /// <summary>
    /// Gets whether any tab is currently visible.
    /// </summary>
    bool HasAnyTabVisible { get; }

    /// <summary>
    /// Shows the File Details tab and switches to it.
    /// </summary>
    void ShowFileDetailsTab();

    /// <summary>
    /// Shows the Search tab and switches to it.
    /// </summary>
    void ShowSearchTab();

    /// <summary>
    /// Shows the Comparison tab and switches to it.
    /// </summary>
    void ShowComparisonTab();

    /// <summary>
    /// Closes the File Details tab and switches to the next available tab.
    /// </summary>
    void CloseFileDetailsTab();

    /// <summary>
    /// Closes the Search tab and switches to the next available tab.
    /// </summary>
    void CloseSearchTab();

    /// <summary>
    /// Closes the Comparison tab and switches to the next available tab.
    /// </summary>
    void CloseComparisonTab();
}
