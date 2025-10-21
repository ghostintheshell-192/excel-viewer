using SheetAtlas.Logging.Services;

namespace SheetAtlas.UI.Avalonia.ViewModels
{
    public partial class MainWindowViewModel
    {

        private async Task ShowAboutDialogAsync()
        {
            var version = typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString() ?? "1.0.0";

            var message = $"SheetAtlas - Excel Cross Reference Viewer\n" +
                         $"Version: {version}\n\n" +
                         $"Cross-platform Excel file comparison and analysis tool.\n\n" +
                         $"License: MIT\n" +
                         $"GitHub: github.com/ghostintheshell-192/sheet-atlas\n\n" +
                         $"© 2025 - Built with .NET 8 and Avalonia UI";

            await _dialogService.ShowInformationAsync(message, "About");
            _logger.LogInfo("Displayed About dialog", "MainWindowViewModel");
        }

        private async Task OpenDocumentationAsync()
        {
            const string documentationUrl = "https://github.com/ghostintheshell-192/sheet-atlas/blob/main/README.md";

            try
            {
                // Open URL in default browser (cross-platform)
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = documentationUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);

                _activityLog.LogInfo("Documentation opened in browser", "Help");
                _logger.LogInfo($"Opened documentation URL: {documentationUrl}", "MainWindowViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to open documentation URL", ex, "MainWindowViewModel");
                _activityLog.LogError("Unable to open documentation", ex, "Help");

                await _dialogService.ShowErrorAsync(
                    $"Unable to open browser.\n\n" +
                    $"You can access the documentation manually:\n{documentationUrl}",
                    "Browser Open Error"
                );
            }

            await Task.CompletedTask;
        }

        private async Task OpenErrorLogAsync()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDirectory = Path.Combine(appDataPath, "SheetAtlas", "Logs");
            var logFile = Path.Combine(logDirectory, string.Format("app-{0:yyyy-MM-dd}.log", DateTime.Now));

            if (!File.Exists(logFile))
            {
                await _dialogService.ShowInformationAsync(
                    "No error log found for today.\n\nLogs are created when errors occur.",
                    "Error Log"
                );
                _activityLog.LogInfo("Error log viewer opened - no log file found", "Help");
                return;
            }

            try
            {
                // Open log file with system default editor (cross-platform)
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = logFile,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);

                _activityLog.LogInfo($"Error log opened: {logFile}", "Help");
                _logger.LogInfo($"Opened error log file: {logFile}", "MainWindowViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to open error log file", ex, "MainWindowViewModel");
                _activityLog.LogError("Unable to open error log", ex, "Help");

                await _dialogService.ShowErrorAsync(
                    $"Unable to open log file.\n\nPath: {logFile}\n\n" +
                    $"You can navigate to the file manually.",
                    "Error Opening Log"
                );
            }
        }

    }
}
