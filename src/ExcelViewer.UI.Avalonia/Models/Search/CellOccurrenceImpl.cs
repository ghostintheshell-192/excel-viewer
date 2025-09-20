using System.Collections.ObjectModel;
using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Models.Search;

public class CellOccurrenceImpl : ViewModelBase, ICellOccurrence
{
    private bool _isSelected;
    private bool _isExpanded;
    private bool _isVisible = true;
    private readonly IReadOnlyDictionary<string, string> _context;

    public int Row { get; }
    public int Column { get; }
    public string Value { get; }
    public ISheetOccurrence ParentSheet { get; }

    public IReadOnlyDictionary<string, string> Context => _context;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

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

    // Calculated properties
    public string FilePath => ParentSheet?.FileOccurrence?.File?.FilePath ?? string.Empty;
    public string FileName => ParentSheet?.FileOccurrence?.File?.FileName ?? string.Empty;
    public string SheetName => ParentSheet?.SheetName ?? string.Empty;
    public string Location => $"Cell [{Row + 1},{Column + 1}]";

    public CellOccurrenceImpl(ISheetOccurrence parentSheet, int row, int column, string value, Dictionary<string, string> context)
    {
        ParentSheet = parentSheet;
        Row = row;
        Column = column;
        Value = value;
        _context = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(context));
    }
}