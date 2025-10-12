using System.Collections.ObjectModel;
using SheetAtlas.UI.Avalonia.ViewModels;

namespace SheetAtlas.UI.Avalonia.Models.Search;

public class FileOccurrenceImpl : ViewModelBase, IFileOccurrence
{
    private bool _isExpanded;
    private bool _isVisible = true;
    private bool _isSelected;
    private bool _isMatchedOnFileName;
    private readonly ObservableCollection<ISheetOccurrence> _sheetOccurrences = new();

    public IFileLoadResultViewModel File { get; }

    public IReadOnlyList<ISheetOccurrence> SheetOccurrences =>
        new ReadOnlyObservableCollection<ISheetOccurrence>(_sheetOccurrences);

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetField(ref _isSelected, value) && _isSelected)
            {
                // When file is selected, select all its sheets
                foreach (var sheet in SheetOccurrences)
                {
                    sheet.IsSelected = value;
                }
            }
        }
    }

    public bool IsMatchedOnFileName => _isMatchedOnFileName;

    public void SetMatchedOnFileName()
    {
        if (!_isMatchedOnFileName)
        {
            _isMatchedOnFileName = true;
            OnPropertyChanged(nameof(IsMatchedOnFileName));
        }
    }

    public FileOccurrenceImpl(IFileLoadResultViewModel file)
    {
        File = file;
        IsExpanded = true; // Expand by default
    }

    public ISheetOccurrence GetOrAddSheetOccurrence(string sheetName)
    {
        var existing = SheetOccurrences.FirstOrDefault(s => s.SheetName == sheetName);
        if (existing != null)
            return existing;

        var newSheet = new SheetOccurrenceImpl(this, sheetName);
        _sheetOccurrences.Add(newSheet);
        return newSheet;
    }
}
