using SheetAtlas.Core.Domain.Entities;

namespace SheetAtlas.UI.Avalonia.ViewModels;

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
