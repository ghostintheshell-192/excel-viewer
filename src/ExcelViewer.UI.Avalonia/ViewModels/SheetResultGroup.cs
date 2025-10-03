using System.Collections.ObjectModel;
using ExcelViewer.Core.Domain.Entities;

namespace ExcelViewer.UI.Avalonia.ViewModels;

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
