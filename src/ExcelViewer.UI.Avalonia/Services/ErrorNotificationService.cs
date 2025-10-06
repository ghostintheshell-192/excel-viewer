using ExcelViewer.Core.Application.Interfaces;
using ExcelViewer.Core.Domain.ValueObjects;

namespace ExcelViewer.UI.Avalonia.Services
{
    /// <summary>
    /// UI-layer service for displaying errors to users.
    /// Bridges exception handling with dialog presentation.
    /// </summary>
    public interface IErrorNotificationService
    {
        /// <summary>
        /// Displays an exception to the user in a friendly way
        /// </summary>
        Task ShowExceptionAsync(Exception exception, string context);

        /// <summary>
        /// Displays an ExcelError to the user
        /// </summary>
        Task ShowErrorAsync(ExcelError error);

        /// <summary>
        /// Displays multiple errors (e.g., from partial file load)
        /// </summary>
        Task ShowErrorsAsync(IEnumerable<ExcelError> errors, string title);
    }

    public class ErrorNotificationService : IErrorNotificationService
    {
        private readonly IDialogService _dialogService;
        private readonly IExceptionHandler _exceptionHandler;

        public ErrorNotificationService(
            IDialogService dialogService,
            IExceptionHandler exceptionHandler)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        public async Task ShowExceptionAsync(Exception exception, string context)
        {
            var error = _exceptionHandler.Handle(exception, context);
            await ShowErrorAsync(error);
        }

        public async Task ShowErrorAsync(ExcelError error)
        {
            var title = error.Level switch
            {
                ErrorLevel.Critical => "Errore Critico",
                ErrorLevel.Error => "Errore",
                ErrorLevel.Warning => "Avviso",
                ErrorLevel.Info => "Informazione",
                _ => "Errore"
            };

            var message = FormatErrorMessage(error);

            if (error.Level == ErrorLevel.Warning)
            {
                await _dialogService.ShowWarningAsync(message, title);
            }
            else if (error.Level == ErrorLevel.Info)
            {
                await _dialogService.ShowInformationAsync(message, title);
            }
            else
            {
                await _dialogService.ShowErrorAsync(message, title);
            }
        }

        public async Task ShowErrorsAsync(IEnumerable<ExcelError> errors, string title)
        {
            var errorList = errors.ToList();
            if (!errorList.Any())
                return;

            // Group by level
            var criticalErrors = errorList.Where(e => e.Level == ErrorLevel.Critical).ToList();
            var regularErrors = errorList.Where(e => e.Level == ErrorLevel.Error).ToList();
            var warnings = errorList.Where(e => e.Level == ErrorLevel.Warning).ToList();

            var message = BuildMultiErrorMessage(criticalErrors, regularErrors, warnings);

            if (criticalErrors.Any() || regularErrors.Any())
            {
                await _dialogService.ShowErrorAsync(message, title);
            }
            else
            {
                await _dialogService.ShowWarningAsync(message, title);
            }
        }

        private string FormatErrorMessage(ExcelError error)
        {
            var message = error.Message;

            if (error.Location != null)
            {
                message += $"\n\nPosizione: {error.Location.ToExcelNotation()}";
            }

            if (!string.IsNullOrEmpty(error.Context))
            {
                message += $"\nContesto: {error.Context}";
            }

            return message;
        }

        private string BuildMultiErrorMessage(
            List<ExcelError> criticalErrors,
            List<ExcelError> regularErrors,
            List<ExcelError> warnings)
        {
            var lines = new List<string>();

            if (criticalErrors.Any())
            {
                lines.Add("❌ Errori Critici:");
                lines.AddRange(criticalErrors.Select(e => $"  • {e.Message}"));
                lines.Add("");
            }

            if (regularErrors.Any())
            {
                lines.Add("⚠️ Errori:");
                lines.AddRange(regularErrors.Select(e => $"  • {e.Message}"));
                lines.Add("");
            }

            if (warnings.Any())
            {
                lines.Add("ℹ️ Avvisi:");
                lines.AddRange(warnings.Select(e => $"  • {e.Message}"));
            }

            return string.Join("\n", lines).Trim();
        }
    }
}
