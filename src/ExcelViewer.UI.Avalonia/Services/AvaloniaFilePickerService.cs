using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.UI.Avalonia.Services;

public class AvaloniaFilePickerService : IFilePickerService
{
    private readonly ILogger<AvaloniaFilePickerService> _logger;

    public AvaloniaFilePickerService(ILogger<AvaloniaFilePickerService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private IStorageProvider? GetStorageProvider()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.StorageProvider;
        }
        return null;
    }

    public async Task<IEnumerable<string>?> OpenFilesAsync(string title, string[]? fileTypeFilters = null)
    {
        try
        {
            var storageProvider = GetStorageProvider();
            if (storageProvider == null)
            {
                _logger.LogWarning("StorageProvider not available for file picker");
                return null;
            }

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true
            };

            if (fileTypeFilters != null && fileTypeFilters.Any())
            {
                var fileTypes = new List<FilePickerFileType>();

                // Excel files filter
                if (fileTypeFilters.Any(f => f.Contains("xlsx") || f.Contains("xls")))
                {
                    fileTypes.Add(new FilePickerFileType("Excel Files")
                    {
                        Patterns = new[] { "*.xlsx", "*.xls" }
                    });
                }

                // All files filter
                fileTypes.Add(FilePickerFileTypes.All);

                options.FileTypeFilter = fileTypes;
            }

            var result = await storageProvider.OpenFilePickerAsync(options);

            if (result == null || !result.Any())
            {
                _logger.LogInformation("User cancelled file picker or selected no files");
                return null;
            }

            var filePaths = result.Select(f => f.Path.LocalPath).ToList();
            _logger.LogInformation("User selected {FileCount} files", filePaths.Count);

            return filePaths;
        }
        catch (OperationCanceledException)
        {
            // User cancelled - this is normal operation
            _logger.LogInformation("File picker cancelled by user");
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            // Permission denied to access file system
            _logger.LogError(ex, "Access denied when opening file picker");
            return null;
        }
        catch (Exception ex)
        {
            // Unexpected errors - platform issues, file system errors, etc.
            _logger.LogError(ex, "Unexpected error opening file picker");
            return null;
        }
    }

    public async Task<string?> SaveFileAsync(string title, string? defaultExtension = null, string[]? fileTypeFilters = null)
    {
        try
        {
            var storageProvider = GetStorageProvider();
            if (storageProvider == null)
            {
                _logger.LogWarning("StorageProvider not available for save file picker");
                return null;
            }

            var options = new FilePickerSaveOptions
            {
                Title = title
            };

            if (!string.IsNullOrEmpty(defaultExtension))
            {
                options.DefaultExtension = defaultExtension;
            }

            if (fileTypeFilters != null && fileTypeFilters.Any())
            {
                var fileTypes = new List<FilePickerFileType>();

                foreach (var filter in fileTypeFilters)
                {
                    var extension = filter.Replace("*", "").Replace(".", "");
                    fileTypes.Add(new FilePickerFileType($"{extension.ToUpper()} Files")
                    {
                        Patterns = new[] { filter }
                    });
                }

                fileTypes.Add(FilePickerFileTypes.All);
                options.FileTypeChoices = fileTypes;
            }

            var result = await storageProvider.SaveFilePickerAsync(options);

            if (result == null)
            {
                _logger.LogInformation("User cancelled save file picker");
                return null;
            }

            var filePath = result.Path.LocalPath;
            _logger.LogInformation("User selected save location: {FilePath}", filePath);

            return filePath;
        }
        catch (OperationCanceledException)
        {
            // User cancelled - this is normal operation
            _logger.LogInformation("Save file picker cancelled by user");
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            // Permission denied to access file system
            _logger.LogError(ex, "Access denied when opening save file picker");
            return null;
        }
        catch (Exception ex)
        {
            // Unexpected errors - platform issues, file system errors, etc.
            _logger.LogError(ex, "Unexpected error opening save file picker");
            return null;
        }
    }
}
