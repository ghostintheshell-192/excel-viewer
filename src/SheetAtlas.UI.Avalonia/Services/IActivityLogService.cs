using System;
using System.Collections.ObjectModel;

namespace SheetAtlas.UI.Avalonia.Services
{
    /// <summary>
    /// Service for logging application activities and operations.
    /// Maintains a timeline of events that can be displayed to the user.
    /// </summary>
    public interface IActivityLogService
    {
        /// <summary>
        /// Read-only collection of activity entries, ordered by timestamp
        /// </summary>
        ReadOnlyObservableCollection<ActivityEntry> Entries { get; }

        /// <summary>
        /// Log an informational message
        /// </summary>
        void LogInfo(string message, string? context = null);

        /// <summary>
        /// Log a warning message
        /// </summary>
        void LogWarning(string message, string? context = null);

        /// <summary>
        /// Log an error message with optional exception
        /// </summary>
        void LogError(string message, Exception? exception = null, string? context = null);

        /// <summary>
        /// Log a fatal/critical error with optional exception
        /// </summary>
        void LogFatal(string message, Exception? exception = null, string? context = null);

        /// <summary>
        /// Clear all entries from the log
        /// </summary>
        void Clear();

        /// <summary>
        /// Event raised when a new entry is added
        /// </summary>
        event EventHandler<ActivityEntry>? EntryAdded;
    }

    public class ActivityLogService : IActivityLogService
    {
        private const int MaxEntries = 100;

        private readonly ObservableCollection<ActivityEntry> _entries = new();
        private readonly ReadOnlyObservableCollection<ActivityEntry> _readOnlyEntries;

        public ReadOnlyObservableCollection<ActivityEntry> Entries => _readOnlyEntries;

        public event EventHandler<ActivityEntry>? EntryAdded;

        public ActivityLogService()
        {
            _readOnlyEntries = new ReadOnlyObservableCollection<ActivityEntry>(_entries);
        }

        public void LogInfo(string message, string? context = null)
        {
            AddEntry(new ActivityEntry(ActivityLogLevel.Info, message, null, context));
        }

        public void LogWarning(string message, string? context = null)
        {
            AddEntry(new ActivityEntry(ActivityLogLevel.Warning, message, null, context));
        }

        public void LogError(string message, Exception? exception = null, string? context = null)
        {
            AddEntry(new ActivityEntry(ActivityLogLevel.Error, message, exception, context));
        }

        public void LogFatal(string message, Exception? exception = null, string? context = null)
        {
            AddEntry(new ActivityEntry(ActivityLogLevel.Fatal, message, exception, context));
        }

        public void Clear()
        {
            _entries.Clear();
        }

        private void AddEntry(ActivityEntry entry)
        {
            _entries.Add(entry);

            // Rimuovi le più vecchie se supera il limite
            while (_entries.Count > MaxEntries)
            {
                _entries.RemoveAt(0);
            }

            // Notifica i subscribers
            EntryAdded?.Invoke(this, entry);
        }
    }

    /// <summary>
    /// Represents a single activity entry in the application timeline
    /// </summary>
    public class ActivityEntry
    {
        /// <summary>
        /// When this activity occurred
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The activity message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Severity level of this activity
        /// </summary>
        public ActivityLogLevel Level { get; }

        /// <summary>
        /// Optional exception associated with this activity
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Optional context describing where/what operation this relates to
        /// </summary>
        public string? Context { get; }

        public ActivityEntry(ActivityLogLevel level, string message, Exception? exception = null, string? context = null)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Exception = exception;
            Context = context;
        }

        /// <summary>
        /// Formatted display string for UI binding
        /// </summary>
        public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");

        /// <summary>
        /// Color hint for UI display based on severity level
        /// </summary>
        public string LevelColor => Level switch
        {
            ActivityLogLevel.Info => "#3B82F6",      // Blue
            ActivityLogLevel.Warning => "#F59E0B",   // Amber/Orange
            ActivityLogLevel.Error => "#EF4444",     // Red
            ActivityLogLevel.Fatal => "#DC2626",     // Dark Red
            _ => "#6B7280"                           // Gray (fallback)
        };

        /// <summary>
        /// Short label for the level
        /// </summary>
        public string LevelLabel => Level switch
        {
            ActivityLogLevel.Info => "INFO",
            ActivityLogLevel.Warning => "WARN",
            ActivityLogLevel.Error => "ERROR",
            ActivityLogLevel.Fatal => "FATAL",
            _ => "LOG"
        };

        /// <summary>
        /// Returns a formatted string representation of this entry
        /// </summary>
        public override string ToString()
        {
            var contextPart = !string.IsNullOrEmpty(Context) ? $" [{Context}]" : "";
            var exceptionPart = Exception != null ? $" ({Exception.GetType().Name})" : "";
            return $"{FormattedTimestamp} [{LevelLabel}] {Message}{contextPart}{exceptionPart}";
        }
    }

    /// <summary>
    /// Severity levels for activity log entries
    /// </summary>
    public enum ActivityLogLevel
    {
        Info,
        Warning,
        Error,
        Fatal
    }
}
