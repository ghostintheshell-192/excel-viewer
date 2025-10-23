using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Logging.Models;

namespace SheetAtlas.UI.Avalonia.ViewModels;

public class FileLoadResultViewModel : ViewModelBase, IFileLoadResultViewModel, IDisposable
{
    private ExcelFile? _file;
    private bool _disposed = false;
    private bool _isExpanded = false;

    public string FilePath => _file?.FilePath ?? string.Empty;
    public string FileName => _file?.FileName ?? string.Empty;
    public LoadStatus Status => _file?.Status ?? LoadStatus.Failed;
    public ExcelFile? File => _file;
    public bool HasErrors => _file?.HasErrors ?? false;
    public bool HasWarnings => _file?.HasWarnings ?? false;
    public bool HasCriticalErrors => _file?.HasCriticalErrors ?? false;

    // Only show Critical, Error, Warning in UI - filter out Info messages
    public IReadOnlyList<ExcelError> Errors
    {
        get
        {
            if (_file?.Errors == null)
                return Array.Empty<ExcelError>();

            return _file.Errors
                .Where(e => e.Level == LogSeverity.Critical ||
                            e.Level == LogSeverity.Error ||
                            e.Level == LogSeverity.Warning)
                .ToList();
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public FileLoadResultViewModel(ExcelFile file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Dispose the ExcelFile (which disposes DataTables)
        _file?.Dispose();

        // Null the reference to allow GC
        _file = null;

        _disposed = true;
    }
}

public class ExcelErrorViewModel : IExcelErrorViewModel
{
    private readonly ExcelError _error;

    public LogSeverity Level => _error.Level;
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
