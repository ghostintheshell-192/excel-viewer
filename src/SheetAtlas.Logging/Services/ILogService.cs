using SheetAtlas.Logging.Models;

namespace SheetAtlas.Logging.Services
{
    /// <summary>
    /// Service for managing application log messages
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Adds a log message to the in-memory log
        /// </summary>
        /// <param name="message">The log message to add</param>
        void AddLogMessage(LogMessage message);

        /// <summary>
        /// Removes a specific log message by ID
        /// </summary>
        /// <param name="messageId">The ID of the message to remove</param>
        void ClearMessage(Guid messageId);

        /// <summary>
        /// Clears all log messages
        /// </summary>
        void ClearAllMessages();

        /// <summary>
        /// Gets all log messages currently stored
        /// </summary>
        IReadOnlyList<LogMessage> GetMessages();

        /// <summary>
        /// Gets the count of stored messages
        /// </summary>
        int UnreadCount { get; }

        /// <summary>
        /// Event raised when a new log message is added
        /// </summary>
        event EventHandler<LogMessage>? MessageAdded;

        /// <summary>
        /// Event raised when all messages are cleared
        /// </summary>
        event EventHandler? MessagesCleared;
    }
}
