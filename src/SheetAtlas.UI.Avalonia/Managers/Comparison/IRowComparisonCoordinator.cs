using System.Collections.ObjectModel;
using System.ComponentModel;
using SheetAtlas.Core.Application.DTOs;
using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.UI.Avalonia.ViewModels;

namespace SheetAtlas.UI.Avalonia.Managers.Comparison;

/// <summary>
/// Manages the lifecycle of row comparison ViewModels.
/// Handles creation, selection, and removal of row comparisons.
/// </summary>
public interface IRowComparisonCoordinator : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the read-only collection of active row comparisons.
    /// </summary>
    ReadOnlyObservableCollection<RowComparisonViewModel> RowComparisons { get; }

    /// <summary>
    /// Gets or sets the currently selected row comparison.
    /// Raises PropertyChanged when changed.
    /// </summary>
    RowComparisonViewModel? SelectedComparison { get; set; }

    /// <summary>
    /// Creates a new row comparison ViewModel and adds it to the collection.
    /// Automatically selects the newly created comparison.
    /// </summary>
    /// <param name="comparison">The row comparison data from the domain layer</param>
    void CreateComparison(RowComparison comparison);

    /// <summary>
    /// Removes a row comparison from the collection.
    /// If the removed comparison was selected, clears the selection.
    /// </summary>
    /// <param name="comparison">The comparison ViewModel to remove</param>
    void RemoveComparison(RowComparisonViewModel comparison);

    /// <summary>
    /// Raised when a comparison is added to the collection.
    /// </summary>
    event EventHandler<ComparisonAddedEventArgs>? ComparisonAdded;

    /// <summary>
    /// Raised when a comparison is removed from the collection.
    /// </summary>
    event EventHandler<ComparisonRemovedEventArgs>? ComparisonRemoved;

    /// <summary>
    /// Raised when the selected comparison changes.
    /// </summary>
    event EventHandler<ComparisonSelectionChangedEventArgs>? SelectionChanged;
}

/// <summary>
/// Event args for comparison added event.
/// </summary>
public class ComparisonAddedEventArgs : EventArgs
{
    public RowComparisonViewModel Comparison { get; }

    public ComparisonAddedEventArgs(RowComparisonViewModel comparison)
    {
        Comparison = comparison;
    }
}

/// <summary>
/// Event args for comparison removed event.
/// </summary>
public class ComparisonRemovedEventArgs : EventArgs
{
    public RowComparisonViewModel Comparison { get; }

    public ComparisonRemovedEventArgs(RowComparisonViewModel comparison)
    {
        Comparison = comparison;
    }
}

/// <summary>
/// Event args for selection changed event.
/// </summary>
public class ComparisonSelectionChangedEventArgs : EventArgs
{
    public RowComparisonViewModel? OldSelection { get; }
    public RowComparisonViewModel? NewSelection { get; }

    public ComparisonSelectionChangedEventArgs(RowComparisonViewModel? oldSelection, RowComparisonViewModel? newSelection)
    {
        OldSelection = oldSelection;
        NewSelection = newSelection;
    }
}
