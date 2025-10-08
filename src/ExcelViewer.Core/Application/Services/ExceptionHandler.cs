using ExcelViewer.Core.Application.Interfaces;
using ExcelViewer.Core.Domain.Exceptions;
using ExcelViewer.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.Core.Application.Services
{
    /// <summary>
    /// Centralized exception handling implementation.
    /// Converts technical exceptions to user-friendly messages and logs details.
    /// </summary>
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ExcelError Handle(Exception exception, string context)
        {
            // Log technical details
            _logger.LogError(exception, "Error in {Context}: {Message}", context, exception.Message);

            // Convert to user-friendly error
            return exception switch
            {
                // Domain exceptions (already have user messages)
                ComparisonException compEx => ExcelError.Critical(
                    context,
                    compEx.UserMessage,
                    compEx),

                // Framework exceptions (need translation)
                FileNotFoundException fnfEx => ExcelError.Critical(
                    context,
                    $"File non trovato: {Path.GetFileName(fnfEx.FileName ?? "sconosciuto")}",
                    fnfEx),

                UnauthorizedAccessException uaEx => ExcelError.Critical(
                    context,
                    "Accesso al file negato. Verifica i permessi.",
                    uaEx),

                IOException ioEx => ExcelError.Critical(
                    context,
                    $"Errore di lettura file: {ioEx.Message}",
                    ioEx),

                InvalidOperationException invOpEx => ExcelError.Critical(
                    context,
                    "Operazione non valida sul file Excel.",
                    invOpEx),

                // Generic fallback
                _ => ExcelError.Critical(
                    context,
                    "Errore imprevisto durante l'elaborazione.",
                    exception)
            };
        }

        public string GetUserMessage(Exception exception)
        {
            return exception switch
            {
                ExcelViewerException customEx => customEx.UserMessage,
                FileNotFoundException _ => "File non trovato",
                UnauthorizedAccessException _ => "Accesso negato",
                IOException _ => "Errore di lettura file",
                InvalidOperationException _ => "File Excel corrotto",
                _ => "Errore imprevisto"
            };
        }

        public bool IsRecoverable(Exception exception)
        {
            return exception switch
            {
                // Recoverable: user can fix by selecting different file
                ComparisonException => true,
                FileNotFoundException => true,
                IOException => true,

                // Not recoverable: programming errors
                ArgumentNullException => false,
                NullReferenceException => false,
                InvalidCastException => false,

                // Default: treat custom exceptions as recoverable
                ExcelViewerException => true,
                _ => false
            };
        }
    }
}
