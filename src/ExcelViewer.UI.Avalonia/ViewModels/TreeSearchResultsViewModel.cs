using System.Collections.ObjectModel;
using System.Windows.Input;
using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

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

public class SearchHistoryItem : ViewModelBase
{
    private bool _isExpanded = true;
    private ObservableCollection<FileResultGroup> _fileGroups = new();

    public string Query { get; }
    public int TotalResults { get; }
    public string DisplayText => $"Search: \"{Query}\" ({TotalResults:N0} hits)";

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public ObservableCollection<FileResultGroup> FileGroups
    {
        get => _fileGroups;
        set => SetField(ref _fileGroups, value);
    }

    // Properties for per-search selection management
    public int SelectedCount => FileGroups
        .SelectMany(fg => fg.SheetGroups)
        .SelectMany(sg => sg.Results)
        .Count(item => item.IsSelected && item.CanBeCompared);

    public string SelectionText => SelectedCount switch
    {
        0 => "no selected rows",
        1 => "1 selected row",
        _ => $"{SelectedCount} selected rows"
    };

    public ICommand ClearSelectionCommand { get; private set; }

    // Event to notify when selection changes for this specific search
    public event EventHandler? SelectionChanged;

    public SearchHistoryItem(string query, IReadOnlyList<SearchResult> results)
    {
        Query = query;
        TotalResults = results.Count;

        // Initialize Clear command
        ClearSelectionCommand = new RelayCommand(() => { ClearSelection(); return Task.CompletedTask; });

        // Group results by file
        var fileGroups = results
            .GroupBy(r => r.SourceFile)
            .Select(fileGroup => new FileResultGroup(fileGroup.Key, fileGroup.ToList()))
            .OrderBy(fg => fg.FileName)
            .ToList();

        FileGroups = new ObservableCollection<FileResultGroup>(fileGroups);

        // Setup selection events for all items in this search
        SetupSelectionEvents();
    }

    private void ClearSelection()
    {
        foreach (var fileGroup in FileGroups)
        {
            foreach (var sheetGroup in fileGroup.SheetGroups)
            {
                foreach (var item in sheetGroup.Results)
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }
        NotifySelectionChanged();
    }

    private void SetupSelectionEvents()
    {
        foreach (var fileGroup in FileGroups)
        {
            foreach (var sheetGroup in fileGroup.SheetGroups)
            {
                foreach (var item in sheetGroup.Results)
                {
                    item.SelectionChanged += OnItemSelectionChanged;
                }
            }
        }
    }

    private void OnItemSelectionChanged(object? sender, EventArgs e)
    {
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(SelectionText));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class FileResultGroup : ViewModelBase
{
    private bool _isExpanded = true;
    private ObservableCollection<SheetResultGroup> _sheetGroups = new();

    public ExcelFile File { get; }
    public string FileName => File.FileName;
    public int TotalResults { get; }
    public string DisplayText => $"{FileName} ({TotalResults:N0} hits)";

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public ObservableCollection<SheetResultGroup> SheetGroups
    {
        get => _sheetGroups;
        set => SetField(ref _sheetGroups, value);
    }

    public FileResultGroup(ExcelFile file, List<SearchResult> results)
    {
        File = file;
        TotalResults = results.Count;

        // Group results by sheet
        var sheetGroups = results
            .GroupBy(r => r.SheetName)
            .Select(sheetGroup => new SheetResultGroup(sheetGroup.Key, sheetGroup.ToList()))
            .OrderBy(sg => sg.SheetName)
            .ToList();

        SheetGroups = new ObservableCollection<SheetResultGroup>(sheetGroups);
    }
}

public class SheetResultGroup : ViewModelBase
{
    private bool _isExpanded = false; // Collapsed by default
    private ObservableCollection<SearchResultItem> _results = new();

    public string SheetName { get; }
    public int TotalResults { get; }
    public string DisplayText => $"{SheetName} ({TotalResults:N0} hits)";

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public ObservableCollection<SearchResultItem> Results
    {
        get => _results;
        set => SetField(ref _results, value);
    }

    public SheetResultGroup(string sheetName, List<SearchResult> results)
    {
        SheetName = sheetName;
        TotalResults = results.Count;

        var resultItems = results
            .Take(100) // Limit to first 100 results per sheet
            .Select(r => new SearchResultItem(r))
            .ToList();

        Results = new ObservableCollection<SearchResultItem>(resultItems);
    }

    public void SetupSelectionEvents(Action selectionChangedCallback)
    {
        foreach (var item in Results)
        {
            item.SelectionChanged += (s, e) => selectionChangedCallback();
        }
    }
}

public class SearchResultItem : ViewModelBase
{
    private bool _isSelected;

    public SearchResult Result { get; }
    public string DisplayText { get; }
    public bool CanBeCompared { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetField(ref _isSelected, value))
            {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? SelectionChanged;

    public SearchResultItem(SearchResult result)
    {
        Result = result;
        CanBeCompared = result.Row >= 0 && result.Column >= 0; // Only cell results can be compared

        if (result.Row >= 0 && result.Column >= 0)
        {
            DisplayText = $"R{result.Row + 1}C{result.Column + 1}: {result.Value}";
        }
        else
        {
            // Handle file name or sheet name results
            var context = result.Context.TryGetValue("Type", out var type) ? type.ToString() : "Content";
            DisplayText = $"{context}: {result.Value}";
        }
    }
}