using System.Collections.ObjectModel;
using System.Windows.Input;
using ExcelViewer.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.UI.Avalonia.ViewModels;

public class TreeSearchResultsViewModel : ViewModelBase
{
    private readonly ILogger<TreeSearchResultsViewModel> _logger;
    private ObservableCollection<SearchHistoryItem> _searchHistory = new();

    public ObservableCollection<SearchHistoryItem> SearchHistory
    {
        get => _searchHistory;
        set => SetField(ref _searchHistory, value);
    }

    public ICommand ClearHistoryCommand { get; }

    public TreeSearchResultsViewModel(ILogger<TreeSearchResultsViewModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ClearHistoryCommand = new RelayCommand(() => { ClearHistory(); return Task.CompletedTask; });
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

    public SearchHistoryItem(string query, IReadOnlyList<SearchResult> results)
    {
        Query = query;
        TotalResults = results.Count;

        // Group results by file
        var fileGroups = results
            .GroupBy(r => r.SourceFile)
            .Select(fileGroup => new FileResultGroup(fileGroup.Key, fileGroup.ToList()))
            .OrderBy(fg => fg.FileName)
            .ToList();

        FileGroups = new ObservableCollection<FileResultGroup>(fileGroups);
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
}

public class SearchResultItem : ViewModelBase
{
    public SearchResult Result { get; }
    public string DisplayText { get; }

    public SearchResultItem(SearchResult result)
    {
        Result = result;

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