using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace ExcelViewer.UI.Avalonia.Services;

public class AvaloniaFilePickerService : IFilePickerService
{
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
        var storageProvider = GetStorageProvider();
        if (storageProvider == null)
            return null;

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
        return result?.Select(f => f.Path.LocalPath);
    }

    public async Task<string?> SaveFileAsync(string title, string? defaultExtension = null, string[]? fileTypeFilters = null)
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null)
            return null;

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
        return result?.Path.LocalPath;
    }
}
