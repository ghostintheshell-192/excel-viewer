using SheetAtlas.Logging.Models;

namespace SheetAtlas.Logging.Services
{
    /// <summary>
    /// Service for managing user notifications (toasts, notification center, etc.)
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Displays a toast notification (auto-dismiss after duration)
        /// </summary>
        /// <param name="level">Severity level</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="durationMs">How long to show the toast (milliseconds)</param>
        void ShowToast(LogSeverity level, string title, string message, int durationMs = 4000);

        /// <summary>
        /// Adds a notification to the notification center
        /// </summary>
        /// <param name="notification">The notification to add</param>
        void AddNotification(LogMessage notification);

        /// <summary>
        /// Removes a specific notification by ID
        /// </summary>
        /// <param name="notificationId">The ID of the notification to remove</param>
        void ClearNotification(Guid notificationId);

        /// <summary>
        /// Clears all notifications from the notification center
        /// </summary>
        void ClearAllNotifications();

        /// <summary>
        /// Gets all notifications currently in the notification center
        /// </summary>
        IReadOnlyList<LogMessage> GetNotifications();

        /// <summary>
        /// Gets the count of unread notifications
        /// </summary>
        int UnreadCount { get; }

        /// <summary>
        /// Event raised when a new notification is added
        /// </summary>
        event EventHandler<LogMessage>? NotificationAdded;

        /// <summary>
        /// Event raised when all notifications are cleared
        /// </summary>
        event EventHandler? NotificationsCleared;
    }
}
