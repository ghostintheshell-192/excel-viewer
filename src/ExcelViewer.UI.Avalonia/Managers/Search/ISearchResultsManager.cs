using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.UI.Avalonia.Models.Search;
using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Managers.Search;

/// <summary>
/// Manager for search operations and results
/// </summary>
public interface ISearchResultsManager
{
    // Properties for accessing results
    IReadOnlyList<SearchResult> Results { get; }
    IReadOnlyList<IGroupedSearchResult> GroupedResults { get; }
    IReadOnlyList<string> Suggestions { get; }

    // Search methods
    Task PerformSearchAsync(string query, SearchOptions? options = null);
    void GenerateSuggestions(string query);

    // Method to set the files to search in
    void SetSearchableFiles(IReadOnlyCollection<IFileLoadResultViewModel> files);

    // Events for change notifications
    event EventHandler<EventArgs> ResultsChanged;
    event EventHandler<EventArgs> SuggestionsChanged;

    // Event for grouped results changes
    event EventHandler<GroupedResultsEventArgs> GroupedResultsUpdated;
}

/// <summary>
/// Event arguments for grouped results updates
/// </summary>
public class GroupedResultsEventArgs : EventArgs
{
    public IEnumerable<IGroupedSearchResult> GroupedResults { get; }

    public GroupedResultsEventArgs(IEnumerable<IGroupedSearchResult> results)
    {
        GroupedResults = results;
    }
}
