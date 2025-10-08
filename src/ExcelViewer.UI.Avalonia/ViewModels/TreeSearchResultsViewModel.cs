using System.Collections.ObjectModel;
using System.Windows.Input;
using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using ExcelViewer.UI.Avalonia.Commands;

namespace ExcelViewer.UI.Avalonia.ViewModels;

public class TreeSearchResultsViewModel : ViewModelBase
{
    private readonly ILogger<TreeSearchResultsViewModel> _logger;
    private readonly IRowComparisonService _rowComparisonService;
    private ObservableCollection<SearchHistoryItem> _searchHistory = new();

    public ObservableCollection<SearchHistoryItem> SearchHistory
    {
        get => _searchHistory;
        set => SetField(ref _searchHistory, value);
    }

    public ICommand ClearHistoryCommand { get; }
    public ICommand CompareSelectedRowsCommand { get; }
    public ICommand ClearSelectionCommand { get; }

    // Properties for UI binding
    public IEnumerable<SearchResultItem> SelectedItems =>
        SearchHistory
            .SelectMany(sh => sh.FileGroups)
            .SelectMany(fg => fg.SheetGroups)
            .SelectMany(sg => sg.Results)
            .Where(item => item.IsSelected && item.CanBeCompared);

    public int SelectedCount => SelectedItems.Count();
    public bool CanCompareRows => SelectedCount >= 2;

    // Event for notifying about row comparison creation
    public event EventHandler<RowComparison>? RowComparisonCreated;

    public TreeSearchResultsViewModel(ILogger<TreeSearchResultsViewModel> logger, IRowComparisonService rowComparisonService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rowComparisonService = rowComparisonService ?? throw new ArgumentNullException(nameof(rowComparisonService));

        ClearHistoryCommand = new RelayCommand(() => { ClearHistory(); return Task.CompletedTask; });
        CompareSelectedRowsCommand = new RelayCommand(async () => await CompareSelectedRowsAsync(), () => CanCompareRows);
        ClearSelectionCommand = new RelayCommand(() => { ClearSelection(); return Task.CompletedTask; });
    }

    public void AddSearchResults(string query, IReadOnlyList<SearchResult> results)
    {
        if (string.IsNullOrWhiteSpace(query) || !results.Any())
            return;

        // Remove existing search with same query
        var existing = SearchHistory.FirstOrDefault(s => s.Query.Equals(query, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            SearchHistory.Remove(existing);
        }

        // Create new search history item
        var searchItem = new SearchHistoryItem(query, results);

        // Setup selection change events for the search item itself (to update global counters)
        searchItem.SelectionChanged += (s, e) => NotifySelectionChanged();

        // Setup selection change events for individual sheet groups (existing logic)
        foreach (var fileGroup in searchItem.FileGroups)
        {
            foreach (var sheetGroup in fileGroup.SheetGroups)
            {
                sheetGroup.SetupSelectionEvents(NotifySelectionChanged);
            }
        }

        // Add to top of list
        SearchHistory.Insert(0, searchItem);

        // Keep only last 5 searches
        while (SearchHistory.Count > 5)
        {
            SearchHistory.RemoveAt(SearchHistory.Count - 1);
        }

        _logger.LogInformation("Added search '{Query}' with {ResultCount} results to history", query, results.Count);
    }

    public void ClearHistory()
    {
        SearchHistory.Clear();
        _logger.LogInformation("Cleared search history");
    }

    public void ClearSelection()
    {
        foreach (var item in SelectedItems.ToList())
        {
            item.IsSelected = false;
        }
        NotifySelectionChanged();
        _logger.LogInformation("Cleared row selection");
    }

    private async Task CompareSelectedRowsAsync()
    {
        try
        {
            var selectedResults = SelectedItems.Select(item => item.Result).ToList();

            if (selectedResults.Count < 2)
            {
                _logger.LogWarning("Attempted to compare rows with less than 2 selected items");
                return;
            }

            var request = new RowComparisonRequest(selectedResults.AsReadOnly(),
                $"Row Comparison {DateTime.Now:HH:mm:ss}");

            var comparison = await _rowComparisonService.CreateRowComparisonAsync(request);

            RowComparisonCreated?.Invoke(this, comparison);

            _logger.LogInformation("Created row comparison with {RowCount} rows", comparison.Rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create row comparison");
            // In a real app, you'd show an error message to the user
        }
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(CanCompareRows));
        OnPropertyChanged(nameof(SelectedItems));
        ((RelayCommand)CompareSelectedRowsCommand).RaiseCanExecuteChanged();
    }
}
