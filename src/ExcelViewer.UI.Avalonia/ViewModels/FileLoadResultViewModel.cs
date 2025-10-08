using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.Core.Domain.ValueObjects;

namespace ExcelViewer.UI.Avalonia.ViewModels;

public class FileLoadResultViewModel : ViewModelBase, IFileLoadResultViewModel
{
    private readonly ExcelFile _file;

    public string FilePath => _file.FilePath;
    public string FileName => _file.FileName;
    public LoadStatus Status => _file.Status;
    public ExcelFile File => _file;
    public bool HasErrors => _file.HasErrors;
    public bool HasWarnings => _file.HasWarnings;
    public bool HasCriticalErrors => _file.HasCriticalErrors;

    public IReadOnlyList<ExcelError> Errors => _file.Errors;

    public FileLoadResultViewModel(ExcelFile file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }
}

public class ExcelErrorViewModel : IExcelErrorViewModel
{
    private readonly ExcelError _error;

    public ErrorLevel Level => _error.Level;
    public string Message => _error.Message;
    public string Context => _error.Context;
    public CellReference? Location => _error.Location;
    public DateTime Timestamp => _error.Timestamp;
    public string Details => _error.InnerException?.ToString() ?? string.Empty;

    public ExcelErrorViewModel(ExcelError error)
    {
        _error = error ?? throw new ArgumentNullException(nameof(error));
    }
}
