using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;

namespace SheetAtlas.UI.Avalonia.ViewModels;

public interface IFileLoadResultViewModel
{
    string FilePath { get; }
    string FileName { get; }
    LoadStatus Status { get; }
    bool HasErrors { get; }
    bool HasWarnings { get; }
    bool HasCriticalErrors { get; }
    IReadOnlyList<ExcelError> Errors { get; }
    ExcelFile File { get; }
}

public interface IExcelErrorViewModel
{
    ErrorLevel Level { get; }
    string Message { get; }
    string Context { get; }
    CellReference? Location { get; }
    DateTime Timestamp { get; }
    string Details { get; }
}
