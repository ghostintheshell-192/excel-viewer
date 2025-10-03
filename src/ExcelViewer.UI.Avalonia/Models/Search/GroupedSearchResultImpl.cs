using System.Collections.ObjectModel;
using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Models.Search;

public class GroupedSearchResultImpl : ViewModelBase, IGroupedSearchResult
{
    private bool _isExpanded;
    private bool _foundInAllFiles;
    private bool _hasVisibleDifferences;
    private bool _isSelected;
    private bool _isVisible = true;
    private int _totalOccurrences;
    private readonly ObservableCollection<IFileOccurrence> _fileOccurrences = new();

    public string Value { get; }

    public bool FoundInAllFiles
    {
        get => _foundInAllFiles;
        private set => SetField(ref _foundInAllFiles, value);
    }

    public int TotalOccurrences
    {
        get => _totalOccurrences;
        private set => SetField(ref _totalOccurrences, value);
    }

    public bool HasVisibleDifferences
    {
        get => _hasVisibleDifferences;
        private set => SetField(ref _hasVisibleDifferences, value);
    }

    public IReadOnlyList<IFileOccurrence> FileOccurrences =>
        new ReadOnlyObservableCollection<IFileOccurrence>(_fileOccurrences);

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public GroupedSearchResultImpl(string value)
    {
        Value = value;
        IsExpanded = true; // Expand by default
    }

    public void AddFileOccurrence(IFileOccurrence fileOccurrence)
    {
        _fileOccurrences.Add(fileOccurrence);
    }

    public void UpdateStats(int totalFileCount)
    {
        int visibleFiles = FileOccurrences.Count(f => f.IsVisible);
        FoundInAllFiles = visibleFiles == totalFileCount;

        TotalOccurrences = FileOccurrences
            .SelectMany(f => f.SheetOccurrences)
            .SelectMany(s => s.CellOccurrences)
            .Count();

        UpdateVisibilityStats();
    }

    public void UpdateVisibilityStats()
    {
        int visibleFiles = FileOccurrences.Count(f => f.IsVisible);
        HasVisibleDifferences = visibleFiles > 0 && visibleFiles < FileOccurrences.Count;
    }
}
