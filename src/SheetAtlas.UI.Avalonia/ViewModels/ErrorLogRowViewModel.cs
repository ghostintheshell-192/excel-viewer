using SheetAtlas.Logging.Models;

namespace SheetAtlas.UI.Avalonia.ViewModels;

/// <summary>
/// ViewModel for a single row in the error log table (flat list)
/// </summary>
public class ErrorLogRowViewModel : ViewModelBase
{
    public DateTime Timestamp { get; }
    public LogSeverity LogLevel { get; }
    public string Message { get; }

    public ErrorLogRowViewModel(DateTime timestamp, LogSeverity logLevel, string message)
    {
        Timestamp = timestamp;
        LogLevel = logLevel;
        Message = message ?? string.Empty;
    }

    // Formatted properties for UI binding
    public string TimestampFormatted => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

    public string LogLevelText => LogLevel.ToString();

    public string LogLevelColor
    {
        get
        {
            return LogLevel switch
            {
                LogSeverity.Critical => "#D32F2F",  // Red
                LogSeverity.Error => "#F57C00",     // Orange
                LogSeverity.Warning => "#FBC02D",   // Yellow
                LogSeverity.Info => "#757575",      // Gray
                _ => "#212121"                      // Black (fallback)
            };
        }
    }

    public string LogLevelColorDark
    {
        get
        {
            return LogLevel switch
            {
                LogSeverity.Critical => "#EF5350",  // Light Red
                LogSeverity.Error => "#FF9800",     // Light Orange
                LogSeverity.Warning => "#FFEB3B",   // Light Yellow
                LogSeverity.Info => "#BDBDBD",      // Light Gray
                _ => "#FFFFFF"                      // White (fallback)
            };
        }
    }
}
