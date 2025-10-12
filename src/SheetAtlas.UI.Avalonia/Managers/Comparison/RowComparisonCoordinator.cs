using System.Collections.ObjectModel;
using System.ComponentModel;
using SheetAtlas.Core.Application.DTOs;
using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.UI.Avalonia.ViewModels;
using Microsoft.Extensions.Logging;

namespace SheetAtlas.UI.Avalonia.Managers.Comparison;

/// <summary>
/// Coordinates the lifecycle of row comparison ViewModels.
/// Manages creation, selection, and removal of comparisons.
/// </summary>
public class RowComparisonCoordinator : IRowComparisonCoordinator
{
    private readonly ILogger<RowComparisonCoordinator> _logger;
    private readonly ILogger<RowComparisonViewModel> _comparisonViewModelLogger;
    private readonly IThemeManager _themeManager;

    private readonly ObservableCollection<RowComparisonViewModel> _rowComparisons = new();
    private RowComparisonViewModel? _selectedComparison;

    public ReadOnlyObservableCollection<RowComparisonViewModel> RowComparisons { get; }

    public RowComparisonViewModel? SelectedComparison
    {
        get => _selectedComparison;
        set
        {
            if (_selectedComparison != value)
            {
                var oldSelection = _selectedComparison;
                _selectedComparison = value;
                OnPropertyChanged(nameof(SelectedComparison));

                SelectionChanged?.Invoke(this, new ComparisonSelectionChangedEventArgs(oldSelection, value));
            }
        }
    }

    public event EventHandler<ComparisonAddedEventArgs>? ComparisonAdded;
    public event EventHandler<ComparisonRemovedEventArgs>? ComparisonRemoved;
    public event EventHandler<ComparisonSelectionChangedEventArgs>? SelectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public RowComparisonCoordinator(
        ILogger<RowComparisonCoordinator> logger,
        ILogger<RowComparisonViewModel> comparisonViewModelLogger,
        IThemeManager themeManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _comparisonViewModelLogger = comparisonViewModelLogger ?? throw new ArgumentNullException(nameof(comparisonViewModelLogger));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));

        RowComparisons = new ReadOnlyObservableCollection<RowComparisonViewModel>(_rowComparisons);
    }

    public void CreateComparison(RowComparison comparison)
    {
        if (comparison == null)
        {
            _logger.LogWarning("CreateComparison called with null comparison");
            return;
        }

        var comparisonViewModel = new RowComparisonViewModel(comparison, _comparisonViewModelLogger, _themeManager);

        // Wire up close event
        comparisonViewModel.CloseRequested += OnComparisonCloseRequested;

        _rowComparisons.Add(comparisonViewModel);
        SelectedComparison = comparisonViewModel; // Auto-select new comparison

        _logger.LogInformation("Created row comparison: {ComparisonName} with {RowCount} rows",
            comparison.Name, comparison.Rows.Count);

        ComparisonAdded?.Invoke(this, new ComparisonAddedEventArgs(comparisonViewModel));
    }

    public void RemoveComparison(RowComparisonViewModel comparison)
    {
        if (comparison == null)
        {
            _logger.LogWarning("RemoveComparison called with null comparison");
            return;
        }

        if (!_rowComparisons.Contains(comparison))
        {
            _logger.LogWarning("Attempted to remove comparison not in collection: {ComparisonName}", comparison.Title);
            return;
        }

        // Unsubscribe from events
        comparison.CloseRequested -= OnComparisonCloseRequested;

        _rowComparisons.Remove(comparison);
        _logger.LogInformation("Removed row comparison: {ComparisonName}", comparison.Title);

        // Clear selection if this was the selected comparison
        if (SelectedComparison == comparison)
        {
            SelectedComparison = null;
        }

        ComparisonRemoved?.Invoke(this, new ComparisonRemovedEventArgs(comparison));
    }

    public void RemoveComparisonsForFile(ExcelFile file)
    {
        if (file == null)
            return;

        var comparisonsToRemove = new List<RowComparisonViewModel>();

        // NOTE: Scan-based approach for robustness
        // Future optimization: Consider event-driven tracking if we have 10+ active comparisons
        foreach (var comparisonViewModel in _rowComparisons.ToList())
        {
            if (comparisonViewModel.Comparison == null)
                continue;

            // Check if this comparison contains rows from the removed file
            var hasRemovedFile = comparisonViewModel.Comparison.Rows.Any(row => row.SourceFile == file);

            if (!hasRemovedFile)
                continue;

            // Get rows NOT from the removed file
            var remainingRows = comparisonViewModel.Comparison.Rows
                .Where(row => row.SourceFile != file)
                .ToList();

            if (remainingRows.Count >= 2)
            {
                // Comparison still valid with remaining rows - update it
                var updatedComparison = new RowComparison(
                    remainingRows.AsReadOnly(),
                    comparisonViewModel.Comparison.Name
                );

                // Create new ViewModel with updated comparison
                var newViewModel = new RowComparisonViewModel(updatedComparison, _comparisonViewModelLogger, _themeManager);
                newViewModel.CloseRequested += OnComparisonCloseRequested;

                // Replace old with new
                var index = _rowComparisons.IndexOf(comparisonViewModel);
                _rowComparisons[index] = newViewModel;

                // Update selection if needed
                if (SelectedComparison == comparisonViewModel)
                {
                    SelectedComparison = newViewModel;
                }

                // Unsubscribe from old
                comparisonViewModel.CloseRequested -= OnComparisonCloseRequested;

                _logger.LogInformation("Updated comparison '{Name}': removed rows from {FilePath}, {RemainingCount} rows remaining",
                    updatedComparison.Name, file.FilePath, remainingRows.Count);
            }
            else
            {
                // Less than 2 rows remaining - remove entire comparison
                comparisonsToRemove.Add(comparisonViewModel);
            }
        }

        // Remove comparisons that don't have enough rows
        foreach (var comparison in comparisonsToRemove)
        {
            RemoveComparison(comparison);
        }

        _logger.LogInformation("Processed comparisons for removed file: {FilePath} (removed {RemovedCount} comparisons)",
            file.FilePath, comparisonsToRemove.Count);
    }

    private void OnComparisonCloseRequested(object? sender, EventArgs e)
    {
        if (sender is RowComparisonViewModel comparisonViewModel)
        {
            RemoveComparison(comparisonViewModel);
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
