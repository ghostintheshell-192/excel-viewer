using System.Collections.ObjectModel;
using System.Windows.Input;
using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.UI.Avalonia.Commands;

namespace ExcelViewer.UI.Avalonia.ViewModels;

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
