namespace SheetAtlas.Logging.Models
{
    /// <summary>
    /// Represents a user notification (error, warning, info)
    /// </summary>
    public class LogMessage
    {
        public LogMessage(
            LogSeverity level,
            string title,
            string message,
            string? context = null,
            Exception? exception = null)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Level = level;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Context = context;
            Exception = exception;
            Actions = new List<LogAction>();
        }

        /// <summary>
        /// Unique identifier for this notification
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// When the notification was created (UTC)
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Severity level
        /// </summary>
        public LogSeverity Level { get; }

        /// <summary>
        /// Short title for the notification
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Detailed message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Additional context information (optional)
        /// </summary>
        public string? Context { get; }

        /// <summary>
        /// Associated exception if any (optional)
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Actions that can be performed on this notification
        /// </summary>
        public List<LogAction> Actions { get; }
    }
}
