using System.Collections.ObjectModel;
using SheetAtlas.Core.Domain.Entities;

namespace SheetAtlas.UI.Avalonia.ViewModels;

public class SheetResultGroup : ViewModelBase, IDisposable
{
    private bool _disposed = false;
    private bool _isExpanded = false; // Collapsed by default
    private ObservableCollection<SearchResultItem> _results = new();
    private Action? _selectionChangedCallback;

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
        _selectionChangedCallback = selectionChangedCallback;

        foreach (var item in Results)
        {
            item.SelectionChanged += OnSelectionChanged;
            // item.SelectionChanged += (s, e) => selectionChangedCallback();
        }
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        _selectionChangedCallback?.Invoke();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            foreach (var item in Results)
            {
                item.SelectionChanged -= OnSelectionChanged;
            }

            Results.Clear();
            _selectionChangedCallback = null;
        }

        // Free unmanaged resources (if any)

        _disposed = true;
    }
}
