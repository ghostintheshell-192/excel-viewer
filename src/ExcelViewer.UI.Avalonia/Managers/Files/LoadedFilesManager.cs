using System.Collections.ObjectModel;
using ExcelViewer.Core.Domain.ValueObjects;
using ExcelViewer.Infrastructure.External;
using ExcelViewer.UI.Avalonia.Services;
using ExcelViewer.UI.Avalonia.ViewModels;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.UI.Avalonia.Managers.Files;

/// <summary>
/// Manages the collection of loaded Excel files and their lifecycle.
/// Handles loading, removal, and retry operations for failed loads.
/// </summary>
public class LoadedFilesManager : ILoadedFilesManager
{
    private readonly IExcelReaderService _excelReaderService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<LoadedFilesManager> _logger;

    private readonly ObservableCollection<IFileLoadResultViewModel> _loadedFiles = new();

    public ReadOnlyObservableCollection<IFileLoadResultViewModel> LoadedFiles { get; }

    public event EventHandler<FileLoadedEventArgs>? FileLoaded;
    public event EventHandler<FileRemovedEventArgs>? FileRemoved;
    public event EventHandler<FileLoadFailedEventArgs>? FileLoadFailed;

    public LoadedFilesManager(
        IExcelReaderService excelReaderService,
        IDialogService dialogService,
        ILogger<LoadedFilesManager> logger)
    {
        _excelReaderService = excelReaderService ?? throw new ArgumentNullException(nameof(excelReaderService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        LoadedFiles = new ReadOnlyObservableCollection<IFileLoadResultViewModel>(_loadedFiles);
    }

    public async Task LoadFilesAsync(IEnumerable<string> filePaths)
    {
        if (filePaths == null || !filePaths.Any())
        {
            _logger.LogWarning("LoadFilesAsync called with null or empty file paths");
            return;
        }

        _logger.LogInformation("Loading {FileCount} files", filePaths.Count());

        var loadedExcelFiles = await _excelReaderService.LoadFilesAsync(filePaths);

        foreach (var excelFile in loadedExcelFiles)
        {
            await ProcessLoadedFileAsync(excelFile);
        }

        _logger.LogInformation("Successfully processed {FileCount} files", loadedExcelFiles.Count);
    }

    public void RemoveFile(IFileLoadResultViewModel? file)
    {
        if (file == null)
        {
            _logger.LogWarning("RemoveFile called with null file");
            return;
        }

        if (!_loadedFiles.Contains(file))
        {
            _logger.LogWarning("Attempted to remove file not in collection: {FileName}", file.FileName);
            return;
        }

        _loadedFiles.Remove(file);
        _logger.LogInformation("Removed file: {FileName}", file.FileName);

        FileRemoved?.Invoke(this, new FileRemovedEventArgs(file));
    }

    public async Task RetryLoadAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("RetryLoadAsync called with null or empty file path");
            return;
        }

        _logger.LogInformation("Retrying file load for: {FilePath}", filePath);

        // Remove existing failed file entry
        var existingFile = _loadedFiles.FirstOrDefault(f =>
            f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

        if (existingFile != null)
        {
            RemoveFile(existingFile);
        }

        // Attempt to reload
        var reloadedFiles = await _excelReaderService.LoadFilesAsync(new[] { filePath });

        foreach (var reloadedFile in reloadedFiles)
        {
            await ProcessLoadedFileAsync(reloadedFile);

            if (reloadedFile.Status == LoadStatus.Success)
            {
                _logger.LogInformation("File {FilePath} reloaded successfully", reloadedFile.FilePath);
            }
            else if (reloadedFile.Status == LoadStatus.PartialSuccess)
            {
                _logger.LogWarning("File {FilePath} reloaded with warnings", reloadedFile.FilePath);
            }
            else
            {
                _logger.LogWarning("File {FilePath} reload failed", reloadedFile.FilePath);
            }
        }
    }

    /// <summary>
    /// Processes a loaded Excel file and determines whether to add it to the collection.
    /// Respects the LoadStatus from Core to decide how to handle the file.
    /// </summary>
    private async Task ProcessLoadedFileAsync(Core.Domain.Entities.ExcelFile excelFile)
    {
        // Check for duplicates
        if (LoadedFiles.Any(f => f.FilePath.Equals(excelFile.FilePath, StringComparison.OrdinalIgnoreCase)))
        {
            await _dialogService.ShowMessageAsync(
                $"File {excelFile.FileName} is already loaded.",
                "Duplicate File");
            return;
        }

        // Respect Core's LoadStatus to determine handling strategy
        switch (excelFile.Status)
        {
            case LoadStatus.Success:
                // File loaded successfully - add to collection
                AddFileToCollection(excelFile, hasErrors: false);
                _logger.LogInformation("File loaded successfully: {FileName}", excelFile.FileName);
                break;

            case LoadStatus.PartialSuccess:
                // File loaded with warnings/errors but has usable data - add to collection
                AddFileToCollection(excelFile, hasErrors: true);
                _logger.LogWarning("File loaded with errors: {FileName} - {ErrorCount} errors",
                    excelFile.FileName, excelFile.Errors.Count);
                break;

            case LoadStatus.Failed:
                // File completely failed to load - add to collection so user can see error details
                AddFileToCollection(excelFile, hasErrors: true);

                _logger.LogError("File failed to load: {FileName} - {ErrorCount} errors",
                    excelFile.FileName, excelFile.Errors.Count);

                // Notify listeners of the failure
                var criticalErrors = excelFile.Errors.Where(e => e.Level == ErrorLevel.Critical);
                var errorMessage = criticalErrors.Any()
                    ? criticalErrors.First().Message
                    : "Unknown error";

                FileLoadFailed?.Invoke(this, new FileLoadFailedEventArgs(
                    excelFile.FilePath,
                    new InvalidOperationException(errorMessage)));
                break;

            default:
                _logger.LogWarning("Unknown LoadStatus: {Status} for file {FileName}",
                    excelFile.Status, excelFile.FileName);
                break;
        }
    }

    /// <summary>
    /// Adds a file to the collection and notifies listeners.
    /// </summary>
    private void AddFileToCollection(Core.Domain.Entities.ExcelFile excelFile, bool hasErrors)
    {
        var fileViewModel = new FileLoadResultViewModel(excelFile);
        _loadedFiles.Add(fileViewModel);

        FileLoaded?.Invoke(this, new FileLoadedEventArgs(fileViewModel, hasErrors));
    }
}
