namespace SheetAtlas.UI.Avalonia.Models.Search;

/// <summary>
/// Represents a cell occurrence in search results
/// </summary>
public interface ICellOccurrence : IToggleable
{
    string Value { get; }
    string SheetName { get; }
    string FileName { get; }
    string FilePath { get; }
    int Row { get; }
    int Column { get; }
    string Location { get; }
    IReadOnlyDictionary<string, string> Context { get; }

    // Reference to parent sheet
    ISheetOccurrence ParentSheet { get; }
}
