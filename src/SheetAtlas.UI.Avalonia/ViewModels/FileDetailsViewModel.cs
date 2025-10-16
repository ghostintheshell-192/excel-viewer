using System.Collections.ObjectModel;
using System.Windows.Input;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.UI.Avalonia.Commands;
using SheetAtlas.UI.Avalonia.Models;
using SheetAtlas.Logging.Services;

namespace SheetAtlas.UI.Avalonia.ViewModels;

public class FileDetailsViewModel : ViewModelBase
{
    private readonly ILogService _logger;
    private IFileLoadResultViewModel? _selectedFile;

    public IFileLoadResultViewModel? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (SetField(ref _selectedFile, value))
            {
                UpdateDetails();
            }
        }
    }

    public ObservableCollection<FileDetailProperty> Properties { get; } = new();
    public ObservableCollection<FileDetailAction> Actions { get; } = new();

    public ICommand RemoveFromListCommand { get; }
    public ICommand CleanAllDataCommand { get; }
    public ICommand RemoveNotificationCommand { get; }
    public ICommand TryAgainCommand { get; }
    public ICommand ViewSheetsCommand { get; }

    public FileDetailsViewModel(ILogService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        RemoveFromListCommand = new RelayCommand(() => { ExecuteRemoveFromList(); return Task.CompletedTask; });
        CleanAllDataCommand = new RelayCommand(() => { ExecuteCleanAllData(); return Task.CompletedTask; });
        RemoveNotificationCommand = new RelayCommand(() => { ExecuteRemoveNotification(); return Task.CompletedTask; });
        TryAgainCommand = new RelayCommand(() => { ExecuteTryAgain(); return Task.CompletedTask; });
        ViewSheetsCommand = new RelayCommand(() => { ExecuteViewSheets(); return Task.CompletedTask; });
    }

    private void UpdateDetails()
    {
        Properties.Clear();
        Actions.Clear();

        if (SelectedFile == null) return;

        // Basic file properties
        Properties.Add(new FileDetailProperty("File Name", SelectedFile.FileName));
        Properties.Add(new FileDetailProperty("Status", SelectedFile.Status.ToString()));
        Properties.Add(new FileDetailProperty("File Path", TruncatePath(SelectedFile.FilePath, 50)));
        Properties.Add(new FileDetailProperty("File Size", FormatFileSize(SelectedFile.FilePath)));
        Properties.Add(new FileDetailProperty("Format", GetFileFormat(SelectedFile.FilePath)));

        // Add separator
        Properties.Add(new FileDetailProperty("", ""));

        // Status-specific details
        switch (SelectedFile.Status)
        {
            case LoadStatus.Success:
                AddSuccessDetails();
                AddSuccessActions();
                break;

            case LoadStatus.Failed:
                // No additional details needed - errors are shown in the file panel treeview
                AddFailedActions();
                break;

            case LoadStatus.PartialSuccess:
                AddPartialSuccessDetails();
                AddPartialSuccessActions();
                break;
        }
    }

    private void AddSuccessDetails()
    {
        Properties.Add(new FileDetailProperty("Content", ""));
        Properties.Add(new FileDetailProperty("", ""));

        if (SelectedFile?.File?.Sheets != null)
        {
            var sheetNames = string.Join(", ", SelectedFile.File.Sheets.Keys.Take(3));
            if (SelectedFile.File.Sheets.Count > 3)
                sheetNames += $" (+{SelectedFile.File.Sheets.Count - 3} more)";

            Properties.Add(new FileDetailProperty("Sheets", $"{SelectedFile.File.Sheets.Count} ({sheetNames})"));

            var totalRows = SelectedFile.File.Sheets.Values.Sum(sheet => sheet.RowCount);
            var totalCols = SelectedFile.File.Sheets.Values.Max(sheet => sheet.ColumnCount);

            Properties.Add(new FileDetailProperty("Total Rows", totalRows.ToString()));
            Properties.Add(new FileDetailProperty("Total Cols", totalCols.ToString()));
        }

        Properties.Add(new FileDetailProperty("Data Status", "Searchable"));
    }

    private void AddPartialSuccessDetails()
    {
        Properties.Add(new FileDetailProperty("Load Results", ""));
        Properties.Add(new FileDetailProperty("", ""));

        Properties.Add(new FileDetailProperty("Status", "Partially Loaded"));

        if (SelectedFile?.File?.Errors?.Any() == true)
        {
            Properties.Add(new FileDetailProperty("Warnings", $"{SelectedFile.File.Errors.Count} issues found"));
        }

        if (SelectedFile?.File?.Sheets != null)
        {
            Properties.Add(new FileDetailProperty("Sheets Loaded", SelectedFile.File.Sheets.Count.ToString()));
        }
    }

    private void AddSuccessActions()
    {
        Actions.Add(new FileDetailAction("Remove from List", RemoveFromListCommand, "Remove file from the loaded files list"));
        Actions.Add(new FileDetailAction("Clean All Data", CleanAllDataCommand, "Remove file and clean all associated data"));
        Actions.Add(new FileDetailAction("View Sheets", ViewSheetsCommand, "View detailed sheet information"));
    }

    private void AddFailedActions()
    {
        Actions.Add(new FileDetailAction("Remove Notification", RemoveNotificationCommand, "Remove failed file notification"));
        Actions.Add(new FileDetailAction("Try Again", TryAgainCommand, "Attempt to reload the file"));
    }

    private void AddPartialSuccessActions()
    {
        Actions.Add(new FileDetailAction("Remove from List", RemoveFromListCommand, "Remove file from the loaded files list"));
        Actions.Add(new FileDetailAction("View Details", ViewSheetsCommand, "View what was loaded successfully"));
    }

    private string GetFileFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" => ".xlsx (Excel 2007+)",
            ".xls" => ".xls (Legacy Excel)",
            ".xlsm" => ".xlsm (Excel Macro)",
            ".csv" => ".csv (Comma Separated)",
            _ => $"{extension} (Unknown)"
        };
    }

    private string FormatFileSize(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) return "Unknown";

            var bytes = fileInfo.Length;
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
            return $"{bytes / (1024 * 1024):F1} MB";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string TruncatePath(string path, int maxLength)
    {
        if (path.Length <= maxLength) return path;
        return "..." + path.Substring(path.Length - maxLength + 3);
    }

    private string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength - 3) + "...";
    }

    // Action handlers - these will be implemented to communicate with MainWindowViewModel
    private void ExecuteRemoveFromList()
    {
        _logger.LogInfo($"Remove from list requested for: {SelectedFile?.FileName}", "FileDetailsViewModel");
        // Will be handled by MainWindowViewModel
        RemoveFromListRequested?.Invoke(SelectedFile);
    }

    private void ExecuteCleanAllData()
    {
        _logger.LogInfo($"Clean all data requested for: {SelectedFile?.FileName}", "FileDetailsViewModel");
        CleanAllDataRequested?.Invoke(SelectedFile);
    }

    private void ExecuteRemoveNotification()
    {
        _logger.LogInfo($"Remove notification requested for: {SelectedFile?.FileName}", "FileDetailsViewModel");
        RemoveNotificationRequested?.Invoke(SelectedFile);
    }

    private void ExecuteTryAgain()
    {
        _logger.LogInfo($"Try again requested for: {SelectedFile?.FileName}", "FileDetailsViewModel");
        TryAgainRequested?.Invoke(SelectedFile);
    }

    private void ExecuteViewSheets()
    {
        _logger.LogInfo($"View sheets requested for: {SelectedFile?.FileName}", "FileDetailsViewModel");
        ViewSheetsRequested?.Invoke(SelectedFile);
    }

    // Events to communicate with parent ViewModels
    public event Action<IFileLoadResultViewModel?>? RemoveFromListRequested;
    public event Action<IFileLoadResultViewModel?>? CleanAllDataRequested;
    public event Action<IFileLoadResultViewModel?>? RemoveNotificationRequested;
    public event Action<IFileLoadResultViewModel?>? TryAgainRequested;
    public event Action<IFileLoadResultViewModel?>? ViewSheetsRequested;
}
