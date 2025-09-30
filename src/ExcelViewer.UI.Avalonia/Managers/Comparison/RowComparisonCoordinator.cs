using System.Collections.ObjectModel;
using System.ComponentModel;
using ExcelViewer.Core.Application.DTOs;
using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.UI.Avalonia.ViewModels;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.UI.Avalonia.Managers.Comparison;

/// <summary>
/// Coordinates the lifecycle of row comparison ViewModels.
/// Manages creation, selection, and removal of comparisons.
/// </summary>
public class RowComparisonCoordinator : IRowComparisonCoordinator
{
    private readonly ILogger<RowComparisonCoordinator> _logger;
    private readonly ILogger<RowComparisonViewModel> _comparisonViewModelLogger;

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
        ILogger<RowComparisonViewModel> comparisonViewModelLogger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _comparisonViewModelLogger = comparisonViewModelLogger ?? throw new ArgumentNullException(nameof(comparisonViewModelLogger));

        RowComparisons = new ReadOnlyObservableCollection<RowComparisonViewModel>(_rowComparisons);
    }

    public void CreateComparison(RowComparison comparison)
    {
        if (comparison == null)
        {
            _logger.LogWarning("CreateComparison called with null comparison");
            return;
        }

        var comparisonViewModel = new RowComparisonViewModel(comparison, _comparisonViewModelLogger);

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
