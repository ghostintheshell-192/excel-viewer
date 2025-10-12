using System.Collections.ObjectModel;
using SheetAtlas.UI.Avalonia.ViewModels;

namespace SheetAtlas.UI.Avalonia.Models.Search;

public class SheetOccurrenceImpl : ViewModelBase, ISheetOccurrence
{
    private bool _isExpanded;
    private bool _isSelected;
    private bool _isVisible = true;
    private bool _isMatchedOnSheetName;
    private readonly ObservableCollection<ICellOccurrence> _cellOccurrences = new();

    public string SheetName { get; }

    public IReadOnlyList<ICellOccurrence> CellOccurrences =>
        new ReadOnlyObservableCollection<ICellOccurrence>(_cellOccurrences);

    public IFileOccurrence FileOccurrence { get; }

    public IFileLoadResultViewModel File => FileOccurrence.File;

    public bool IsMatchedOnSheetName => _isMatchedOnSheetName;

    public void SetMatchedOnSheetName()
    {
        if (!_isMatchedOnSheetName)
        {
            _isMatchedOnSheetName = true;
            OnPropertyChanged(nameof(IsMatchedOnSheetName));
        }
    }

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

    public SheetOccurrenceImpl(IFileOccurrence fileOccurrence, string sheetName)
    {
        FileOccurrence = fileOccurrence;
        SheetName = sheetName;
        IsExpanded = true; // Expand by default
    }

    public ICellOccurrence AddCellOccurrence(int row, int column, string value, Dictionary<string, string> context)
    {
        var cellOccurrence = new CellOccurrenceImpl(this, row, column, value, context);
        _cellOccurrences.Add(cellOccurrence);
        return cellOccurrence;
    }
}
