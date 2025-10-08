using ExcelViewer.Core.Domain.ValueObjects;

namespace ExcelViewer.Core.Application.Interfaces
{
    /// <summary>
    /// Centralized exception handling service.
    /// Converts exceptions to user-friendly error messages and logs technical details.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Handles an exception and returns a user-friendly error object
        /// </summary>
        ExcelError Handle(Exception exception, string context);

        /// <summary>
        /// Handles an exception and returns a user-friendly message
        /// </summary>
        string GetUserMessage(Exception exception);

        /// <summary>
        /// Determines if an exception is recoverable
        /// </summary>
        bool IsRecoverable(Exception exception);
    }
}
