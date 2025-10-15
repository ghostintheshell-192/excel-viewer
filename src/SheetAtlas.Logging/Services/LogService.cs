using Microsoft.Extensions.Logging;
using SheetAtlas.Logging.Models;

namespace SheetAtlas.Logging.Services
{
    /// <summary>
    /// In-memory implementation of log service
    /// Manages log message storage and events
    /// </summary>
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;
        private readonly List<LogMessage> _messages;
        private readonly object _lock = new object();

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messages = new List<LogMessage>();
        }

        public int UnreadCount
        {
            get
            {
                lock (_lock)
                {
                    return _messages.Count;
                }
            }
        }

        public event EventHandler<LogMessage>? MessageAdded;
        public event EventHandler? MessagesCleared;

        public void AddLogMessage(LogMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            lock (_lock)
            {
                _messages.Add(message);
            }

            _logger.LogDebug("Log message added: {Title} ({Level})", message.Title, message.Level);

            // Raise event for UI subscribers
            MessageAdded?.Invoke(this, message);
        }

        public void ClearMessage(Guid messageId)
        {
            lock (_lock)
            {
                var message = _messages.FirstOrDefault(n => n.Id == messageId);
                if (message != null)
                {
                    _messages.Remove(message);
                    _logger.LogDebug("Log message cleared: {Id}", messageId);
                }
            }
        }

        public void ClearAllMessages()
        {
            lock (_lock)
            {
                _messages.Clear();
            }

            _logger.LogDebug("All messages cleared");

            // Raise event for UI subscribers
            MessagesCleared?.Invoke(this, EventArgs.Empty);
        }

        public IReadOnlyList<LogMessage> GetMessages()
        {
            lock (_lock)
            {
                // Return a copy to avoid external modifications
                return _messages.ToList().AsReadOnly();
            }
        }
    }
}
