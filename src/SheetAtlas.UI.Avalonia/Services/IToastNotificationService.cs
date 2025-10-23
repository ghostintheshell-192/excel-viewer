using SheetAtlas.Logging.Models;

namespace SheetAtlas.UI.Avalonia.Services
{
    public interface IToastNotificationService
    {
        /// <summary>
        /// Displays a toast notification (auto-dismiss after duration)
        /// </summary>
        /// <param name="level">Severity level</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="durationMs">How long to show the toast (milliseconds)</param>
        void ShowToast(LogSeverity level, string title, string message, int durationMs = 4000);
    }

}
