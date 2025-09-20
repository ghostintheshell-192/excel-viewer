namespace ExcelViewer.UI.Avalonia.Models;

/// <summary>
/// Interface for items that can be toggled (expanded/collapsed or selected/deselected)
/// </summary>
public interface IToggleable
{
    bool IsExpanded { get; set; }
    bool IsSelected { get; set; }
    bool IsVisible { get; set; }
}