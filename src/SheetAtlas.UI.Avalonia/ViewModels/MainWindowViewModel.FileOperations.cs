using SheetAtlas.Logging.Services;

namespace SheetAtlas.UI.Avalonia.ViewModels
{
    public partial class MainWindowViewModel
    {
        private async Task RetryLoadFileAsync(IFileLoadResultViewModel file)
        {
            try
            {
                _activityLog.LogInfo($"Retrying file load: {file.FileName}", "FileRetry");
                _logger.LogInfo($"Retrying file load for: {file.FilePath}", "MainWindowViewModel");

                var filePath = file.FilePath; // Save path before removal

                await _filesManager.RetryLoadAsync(filePath);

                // Re-select the file after retry to maintain focus
                var reloadedFile = _filesManager.LoadedFiles.FirstOrDefault(f =>
                    f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

                if (reloadedFile != null)
                {
                    FileDetailsViewModel.SelectedFile = reloadedFile;
                }

                _activityLog.LogInfo($"Retry completed: {file.FileName}", "FileRetry");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrying file load: {file.FilePath}", ex, "MainWindowViewModel");
                _activityLog.LogError($"Error reloading {file.FileName}", ex, "FileRetry");

                await _dialogService.ShowErrorAsync(
                    $"Unable to reload file '{file.FileName}'.\n\n" +
                    $"Details: {ex.Message}",
                    "Reload Error"
                );
            }
        }

        private async Task LoadFileAsync()
        {
            try
            {
                _activityLog.LogInfo("Opening file selection...", "FileLoad");

                var files = await _filePickerService.OpenFilesAsync("Select Excel Files", new[] { "*.xlsx", "*.xls" });

                if (files?.Any() != true)
                {
                    // User cancelled or didn't select any files - this is normal
                    _activityLog.LogInfo("File selection cancelled by user", "FileLoad");
                    return;
                }

                _activityLog.LogInfo($"Loading {files.Count()} file(s)...", "FileLoad");
                await _filesManager.LoadFilesAsync(files);

                _activityLog.LogInfo($"Loading completed: {files.Count()} file(s)", "FileLoad");
            }
            catch (Exception ex)
            {
                // Safety net for unexpected errors
                // Note: FilePickerService and ExcelReaderService handle their own errors internally
                // This catch is only for truly unexpected issues (OOM, async state corruption, etc.)
                _logger.LogError("Unexpected error when loading files", ex, "MainWindowViewModel");
                _activityLog.LogError("Unexpected error during loading", ex, "FileLoad");

                await _dialogService.ShowErrorAsync(
                    "An unexpected error occurred while loading files.\n\n" +
                    $"Details: {ex.Message}\n\n" +
                    "Operation cancelled.",
                    "Loading Error"
                );
            }
        }

        private async Task UnloadAllFilesAsync()
        {
            if (!LoadedFiles.Any())
                return;

            if (!await ConfirmUnloadAllAsync())
                return;

            _activityLog.LogInfo($"Unloading all {LoadedFiles.Count} file(s)...", "FileUnload");

            ClearAllUIState();
            RemoveAllComparisons();
            ClearAllSearchResults();
            RemoveAllFiles();

            _activityLog.LogInfo("All files unloaded successfully", "FileUnload");
        }

        private async Task<bool> ConfirmUnloadAllAsync()
        {
            return await _dialogService.ShowConfirmationAsync(
                $"Are you sure you want to unload all {LoadedFiles.Count} file(s)?\n\n" +
                "This will clear all data, search results, and comparisons.",
                "Unload All Files");
        }

        private void ClearAllUIState()
        {
            SelectedFile = null;
        }

        private void RemoveAllComparisons()
        {
            var comparisonsToRemove = RowComparisons.ToList();
            foreach (var comparison in comparisonsToRemove)
            {
                _comparisonCoordinator.RemoveComparison(comparison);
            }
        }

        private void ClearAllSearchResults()
        {
            TreeSearchResultsViewModel?.ClearHistory();
            SearchViewModel?.ClearSearchCommand.Execute(null);
        }

        private void RemoveAllFiles()
        {
            var filesToRemove = LoadedFiles.ToList();
            foreach (var file in filesToRemove)
            {
                file.Dispose();
                _filesManager.RemoveFile(file);
            }
            _logger.LogInfo($"Unloaded {filesToRemove.Count} file(s)", "MainWindowViewModel");
        }

    }
}
