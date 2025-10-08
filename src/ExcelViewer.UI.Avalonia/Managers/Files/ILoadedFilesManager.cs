using System.Collections.ObjectModel;
using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Managers.Files;

/// <summary>
/// Manages the collection of loaded Excel files and their lifecycle.
/// Handles loading, removal, and retry operations for failed loads.
/// </summary>
public interface ILoadedFilesManager
{
    /// <summary>
    /// Gets the read-only collection of currently loaded files.
    /// </summary>
    ReadOnlyObservableCollection<IFileLoadResultViewModel> LoadedFiles { get; }

    /// <summary>
    /// Loads Excel files from the specified file paths.
    /// Automatically checks for duplicates and handles errors.
    /// </summary>
    /// <param name="filePaths">Collection of file paths to load</param>
    /// <returns>Task representing the async operation</returns>
    Task LoadFilesAsync(IEnumerable<string> filePaths);

    /// <summary>
    /// Removes a file from the loaded files collection.
    /// </summary>
    /// <param name="file">The file to remove</param>
    void RemoveFile(IFileLoadResultViewModel? file);

    /// <summary>
    /// Retries loading a file that previously failed.
    /// Removes the old failed entry and attempts to reload.
    /// </summary>
    /// <param name="filePath">Path of the file to retry loading</param>
    /// <returns>Task representing the async operation</returns>
    Task RetryLoadAsync(string filePath);

    /// <summary>
    /// Raised when a file is successfully loaded (or loaded with errors).
    /// </summary>
    event EventHandler<FileLoadedEventArgs>? FileLoaded;

    /// <summary>
    /// Raised when a file is removed from the collection.
    /// </summary>
    event EventHandler<FileRemovedEventArgs>? FileRemoved;

    /// <summary>
    /// Raised when a file load operation fails completely.
    /// </summary>
    event EventHandler<FileLoadFailedEventArgs>? FileLoadFailed;
}

/// <summary>
/// Event args for file loaded event.
/// </summary>
public class FileLoadedEventArgs : EventArgs
{
    public IFileLoadResultViewModel File { get; }
    public bool HasErrors { get; }

    public FileLoadedEventArgs(IFileLoadResultViewModel file, bool hasErrors)
    {
        File = file;
        HasErrors = hasErrors;
    }
}

/// <summary>
/// Event args for file removed event.
/// </summary>
public class FileRemovedEventArgs : EventArgs
{
    public IFileLoadResultViewModel File { get; }

    public FileRemovedEventArgs(IFileLoadResultViewModel file)
    {
        File = file;
    }
}

/// <summary>
/// Event args for file load failed event.
/// </summary>
public class FileLoadFailedEventArgs : EventArgs
{
    public string FilePath { get; }
    public Exception Exception { get; }

    public FileLoadFailedEventArgs(string filePath, Exception exception)
    {
        FilePath = filePath;
        Exception = exception;
    }
}
