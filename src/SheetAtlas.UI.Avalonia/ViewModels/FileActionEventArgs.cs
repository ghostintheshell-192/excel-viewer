namespace SheetAtlas.UI.Avalonia.ViewModels;

/// <summary>
/// Event arguments for file-related actions requested from FileDetailsViewModel.
/// Used for actions like Remove, Clean, Retry, etc.
/// </summary>
public class FileActionEventArgs : EventArgs
{
    /// <summary>
    /// The file involved in the action
    /// </summary>
    public IFileLoadResultViewModel? File { get; }

    public FileActionEventArgs(IFileLoadResultViewModel? file)
    {
        File = file;
    }
}
