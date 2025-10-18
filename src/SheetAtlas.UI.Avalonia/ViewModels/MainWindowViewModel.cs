using System.Collections.ObjectModel;
using System.Windows.Input;
using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Core.Application.DTOs;
using SheetAtlas.Infrastructure.External;
using SheetAtlas.UI.Avalonia.Models.Search;
using SheetAtlas.UI.Avalonia.Services;
using SheetAtlas.Logging.Services;
using Microsoft.Extensions.DependencyInjection;
using SheetAtlas.UI.Avalonia.Commands;
using SheetAtlas.UI.Avalonia.Managers;
using SheetAtlas.UI.Avalonia.Managers.Files;
using SheetAtlas.UI.Avalonia.Managers.Comparison;

namespace SheetAtlas.UI.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ILoadedFilesManager _filesManager;
    private readonly IRowComparisonCoordinator _comparisonCoordinator;
    private readonly IFilePickerService _filePickerService;
    private readonly ILogService _logger;
    private readonly IThemeManager _themeManager;
    private readonly IActivityLogService _activityLog;
    private readonly IDialogService _dialogService;

    private IFileLoadResultViewModel? _selectedFile;
    private object? _currentView;
    private int _selectedTabIndex;
    private bool _isSidebarExpanded;

    public ReadOnlyObservableCollection<IFileLoadResultViewModel> LoadedFiles => _filesManager.LoadedFiles;
    public ReadOnlyObservableCollection<RowComparisonViewModel> RowComparisons => _comparisonCoordinator.RowComparisons;

    // Expose SelectedComparison from Coordinator for binding
    public RowComparisonViewModel? SelectedComparison
    {
        get => _comparisonCoordinator.SelectedComparison;
        set => _comparisonCoordinator.SelectedComparison = value;
    }

    public SearchViewModel? SearchViewModel { get; private set; }
    public FileDetailsViewModel? FileDetailsViewModel { get; private set; }
    public TreeSearchResultsViewModel? TreeSearchResultsViewModel { get; private set; }

    public IFileLoadResultViewModel? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (SetField(ref _selectedFile, value))
            {
                // Update FileDetailsViewModel when selection changes
                if (FileDetailsViewModel != null)
                {
                    FileDetailsViewModel.SelectedFile = value;
                }

                // Switch to appropriate tab
                if (value != null)
                {
                    // File selected - switch to File Details tab
                    SelectedTabIndex = 0;
                }
                else
                {
                    // No file selected - switch to Search Results tab
                    SelectedTabIndex = 1;
                }
            }
        }
    }

    public object? CurrentView
    {
        get => _currentView;
        set => SetField(ref _currentView, value);
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetField(ref _selectedTabIndex, value);
    }

    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        set => SetField(ref _isSidebarExpanded, value);
    }

    public IThemeManager ThemeManager { get; }
    public ICommand LoadFileCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ToggleSidebarCommand { get; }
    public ICommand ShowSearchResultsCommand { get; }
    public ICommand ShowAboutCommand { get; }
    public ICommand ShowDocumentationCommand { get; }
    public ICommand ViewErrorLogCommand { get; }

    // Delegated commands from SearchViewModel
    public ICommand ShowAllFilesCommand => SearchViewModel?.ShowAllFilesCommand ?? new RelayCommand(() => Task.CompletedTask);

    public MainWindowViewModel(
        ILoadedFilesManager filesManager,
        IRowComparisonCoordinator comparisonCoordinator,
        IFilePickerService filePickerService,
        ILogService logger,
        IThemeManager themeManager,
        IActivityLogService activityLog,
        IDialogService dialogService)
    {
        _filesManager = filesManager ?? throw new ArgumentNullException(nameof(filesManager));
        _comparisonCoordinator = comparisonCoordinator ?? throw new ArgumentNullException(nameof(comparisonCoordinator));
        _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _activityLog = activityLog ?? throw new ArgumentNullException(nameof(activityLog));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Initialize with Search Results tab selected
        _selectedTabIndex = 1;

        // Initialize sidebar as collapsed (new UX)
        _isSidebarExpanded = false;

        LoadFileCommand = new RelayCommand(async () => await LoadFileAsync());

        ToggleSidebarCommand = new RelayCommand(() =>
        {
            IsSidebarExpanded = !IsSidebarExpanded;
            return Task.CompletedTask;
        });

        ThemeManager = themeManager;
        ToggleThemeCommand = new RelayCommand(() =>
        {
            ThemeManager.ToggleTheme();
            return Task.CompletedTask;
        });

        ShowSearchResultsCommand = new RelayCommand(() =>
        {
            SelectedTabIndex = 1; // Switch to Search Results tab
            return Task.CompletedTask;
        });

        ShowAboutCommand = new RelayCommand(async () => await ShowAboutDialogAsync());
        ShowDocumentationCommand = new RelayCommand(async () => await OpenDocumentationAsync());
        ViewErrorLogCommand = new RelayCommand(async () => await OpenErrorLogAsync());

        // Subscribe to file manager events
        _filesManager.FileLoaded += OnFileLoaded;
        _filesManager.FileRemoved += OnFileRemoved;
        _filesManager.FileLoadFailed += OnFileLoadFailed;

        // Subscribe to comparison coordinator events
        _comparisonCoordinator.SelectionChanged += OnComparisonSelectionChanged;
        _comparisonCoordinator.ComparisonRemoved += OnComparisonRemoved;
        _comparisonCoordinator.PropertyChanged += OnComparisonCoordinatorPropertyChanged;
    }

    private void OnComparisonCoordinatorPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Propagate PropertyChanged from Coordinator to ViewModel
        if (e.PropertyName == nameof(IRowComparisonCoordinator.SelectedComparison))
        {
            OnPropertyChanged(nameof(SelectedComparison));
        }
    }

    private void OnComparisonRemoved(object? sender, ComparisonRemovedEventArgs e)
    {
        // Clear all selections in TreeSearchResultsViewModel
        TreeSearchResultsViewModel?.ClearSelection();

        // Switch back to Search Results tab
        SelectedTabIndex = 1;

        _logger.LogInfo("Comparison removed and selections cleared", "MainWindowViewModel");
    }

    public void SetSearchViewModel(SearchViewModel searchViewModel)
    {
        SearchViewModel = searchViewModel ?? throw new ArgumentNullException(nameof(searchViewModel));
        SearchViewModel.Initialize(LoadedFiles);
        OnPropertyChanged(nameof(ShowAllFilesCommand));

        // Wire up search results to tree view
        if (SearchViewModel != null)
        {
            // Subscribe to search results changes
            SearchViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SearchViewModel.SearchResults) && TreeSearchResultsViewModel != null)
                {
                    var query = SearchViewModel.SearchQuery;
                    var results = SearchViewModel.SearchResults;
                    if (!string.IsNullOrWhiteSpace(query) && results?.Any() == true)
                    {
                        TreeSearchResultsViewModel.AddSearchResults(query, results.ToList());

                        // Switch to Search Results tab to show results
                        SelectedTabIndex = 1;
                    }
                }
            };
        }
    }

    public void SetFileDetailsViewModel(FileDetailsViewModel fileDetailsViewModel)
    {
        FileDetailsViewModel = fileDetailsViewModel ?? throw new ArgumentNullException(nameof(fileDetailsViewModel));

        // Wire up events from FileDetailsViewModel
        FileDetailsViewModel.RemoveFromListRequested += OnRemoveFromListRequested;
        FileDetailsViewModel.CleanAllDataRequested += OnCleanAllDataRequested;
        FileDetailsViewModel.RemoveNotificationRequested += OnRemoveNotificationRequested;
        FileDetailsViewModel.TryAgainRequested += OnTryAgainRequested;

        // Set current selection if any
        FileDetailsViewModel.SelectedFile = SelectedFile;
    }

    public void SetTreeSearchResultsViewModel(TreeSearchResultsViewModel treeSearchResultsViewModel)
    {
        TreeSearchResultsViewModel = treeSearchResultsViewModel ?? throw new ArgumentNullException(nameof(treeSearchResultsViewModel));

        // Wire up row comparison creation
        TreeSearchResultsViewModel.RowComparisonCreated += OnRowComparisonCreated;
    }

    private void OnRowComparisonCreated(object? sender, RowComparison comparison)
    {
        _comparisonCoordinator.CreateComparison(comparison);
    }

    private void OnComparisonSelectionChanged(object? sender, ComparisonSelectionChangedEventArgs e)
    {
        // Switch to comparison tab ONLY when user manually selects a comparison
        // Do NOT switch if we're just updating an existing comparison (e.g., after file removal)
        if (e.NewSelection != null && e.OldSelection == null)
        {
            // User selected a comparison for the first time (not replacing existing)
            SelectedTabIndex = 2; // Comparison tab
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

    // Event handlers for FileDetailsViewModel - delegate to FilesManager
    private void OnRemoveFromListRequested(IFileLoadResultViewModel? file) => _filesManager.RemoveFile(file);

    private void OnCleanAllDataRequested(IFileLoadResultViewModel? file)
    {
        if (file == null)
        {
            _logger.LogWarning("Clean all data requested with null file", "MainWindowViewModel");
            return;
        }

        _logger.LogInfo($"Clean all data requested for: {file.FileName}", "MainWindowViewModel");

        // Clear selection if this file is currently selected (prevent memory leak)
        if (SelectedFile == file)
        {
            SelectedFile = null;
        }

        // Remove search results that reference this file (TreeView history)
        TreeSearchResultsViewModel?.RemoveSearchResultsForFile(file.File);

        // Remove current search results that reference this file (SearchViewModel)
        SearchViewModel?.RemoveResultsForFile(file.File);

        // Remove row comparisons that reference this file
        _comparisonCoordinator.RemoveComparisonsForFile(file.File);

        // Dispose ViewModel (which disposes ExcelFile and DataTables, then nulls the reference)
        file.Dispose();

        // Finally, remove the file from the loaded files list
        _filesManager.RemoveFile(file);

        _logger.LogInfo($"Cleaned all data for file: {file.FileName}", "MainWindowViewModel");

        // AGGRESSIVE CLEANUP: Force garbage collection after file removal
        // REASON: DataTable objects (100-500 MB each) end up in Large Object Heap (LOH)
        // ISSUE: .NET GC is lazy for Gen 2/LOH - can wait minutes before collection
        // IMPACT: Without this, memory stays high even after Dispose() until GC decides to run
        // TODO: When DataTable is replaced with lightweight structures, this can be removed
        //       or changed to standard GC.Collect() without aggressive mode
        Task.Run(() =>
        {
            // Enable LOH compaction for this collection cycle
            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;

            // Force Gen 2 + LOH collection with compaction (blocking in background thread)
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        });
    }

    private void OnRemoveNotificationRequested(IFileLoadResultViewModel? file) => _filesManager.RemoveFile(file);

    private void OnTryAgainRequested(IFileLoadResultViewModel? file)
    {
        if (file == null)
        {
            _logger.LogWarning("Try again requested but file is null", "MainWindowViewModel");
            return;
        }

        // Use fire-and-forget pattern with proper error handling
        _ = RetryLoadFileAsync(file);
    }

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

    // Event handlers for FilesManager events
    private void OnFileLoaded(object? sender, FileLoadedEventArgs e)
    {
        _logger.LogInfo($"File loaded: {e.File.FileName} (HasErrors: {e.HasErrors})", "MainWindowViewModel");
    }

    private void OnFileRemoved(object? sender, FileRemovedEventArgs e)
    {
        _logger.LogInfo($"File removed: {e.File.FileName}", "MainWindowViewModel");
    }

    private void OnFileLoadFailed(object? sender, FileLoadFailedEventArgs e)
    {
        _logger.LogError($"File load failed: {e.FilePath}", e.Exception, "MainWindowViewModel");
    }

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

