using Microsoft.Extensions.Logging;
using SheetAtlas.Logging.Models;

namespace SheetAtlas.Logging.Services
{
    /// <summary>
    /// In-memory implementation of notification service
    /// Manages notification storage and events (UI rendering will be added later)
    /// </summary>
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;
        private readonly List<LogMessage> _notifications;
        private readonly object _lock = new object();

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notifications = new List<LogMessage>();
        }

        public int UnreadCount
        {
            get
            {
                lock (_lock)
                {
                    return _notifications.Count;
                }
            }
        }

        public event EventHandler<LogMessage>? NotificationAdded;
        public event EventHandler? NotificationsCleared;

        public void ShowToast(LogSeverity level, string title, string message, int durationMs = 4000)
        {
            // For now, we just log the toast - UI implementation will come later
            var logLevel = MapNotificationLevelToLogLevel(level);
            _logger.Log(logLevel, "Toast notification: [{Level}] {Title}: {Message}", level, title, message);

            // Also add to notification center so it's not lost
            var msg = new LogMessage(level, title, message);
            AddNotification(msg);
        }

        public void AddNotification(LogMessage notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            lock (_lock)
            {
                _notifications.Add(notification);
            }

            _logger.LogDebug("Notification added: {Title} ({Level})", notification.Title, notification.Level);

            // Raise event for UI subscribers
            NotificationAdded?.Invoke(this, notification);
        }

        public void ClearNotification(Guid notificationId)
        {
            lock (_lock)
            {
                var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    _notifications.Remove(notification);
                    _logger.LogDebug("Notification cleared: {Id}", notificationId);
                }
            }
        }

        public void ClearAllNotifications()
        {
            lock (_lock)
            {
                _notifications.Clear();
            }

            _logger.LogDebug("All notifications cleared");

            // Raise event for UI subscribers
            NotificationsCleared?.Invoke(this, EventArgs.Empty);
        }

        public IReadOnlyList<LogMessage> GetNotifications()
        {
            lock (_lock)
            {
                // Return a copy to avoid external modifications
                return _notifications.ToList().AsReadOnly();
            }
        }

        private LogLevel MapNotificationLevelToLogLevel(LogSeverity level)
        {
            return level switch
            {
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Critical => LogLevel.Critical,
                _ => LogLevel.Information
            };
        }
    }
}
