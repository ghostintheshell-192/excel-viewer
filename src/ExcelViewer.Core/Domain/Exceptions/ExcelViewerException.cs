namespace ExcelViewer.Core.Domain.Exceptions
{
    /// <summary>
    /// Base exception for all Excel Viewer domain exceptions.
    /// Represents business rule violations and domain-specific errors.
    /// </summary>
    public abstract class ExcelViewerException : Exception
    {
        /// <summary>
        /// User-friendly error message suitable for display in UI
        /// </summary>
        public string UserMessage { get; }

        /// <summary>
        /// Error code for logging and telemetry
        /// </summary>
        public string ErrorCode { get; }

        protected ExcelViewerException(
            string message,
            string userMessage,
            string errorCode,
            Exception? innerException = null)
            : base(message, innerException)
        {
            UserMessage = userMessage;
            ErrorCode = errorCode;
        }
    }
}
