using System.Collections.ObjectModel;
using System.Windows.Input;
using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.UI.Avalonia.Services;
using SheetAtlas.Logging.Services;
using SheetAtlas.UI.Avalonia.Commands;
using SheetAtlas.UI.Avalonia.Managers;
using SheetAtlas.UI.Avalonia.Managers.Files;
using SheetAtlas.UI.Avalonia.Managers.Comparison;
using SheetAtlas.UI.Avalonia.Managers.Navigation;
using SheetAtlas.UI.Avalonia.Managers.FileDetails;
using System.ComponentModel;

namespace SheetAtlas.UI.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private bool _disposed = false;
    private readonly ILoadedFilesManager _filesManager;
    private readonly IRowComparisonCoordinator _comparisonCoordinator;
    private readonly ITabNavigationCoordinator _tabNavigator;
    private readonly IFileDetailsCoordinator _fileDetailsCoordinator;
    private readonly IFilePickerService _filePickerService;
    private readonly ILogService _logger;
    private readonly IThemeManager _themeManager;
    private readonly IActivityLogService _activityLog;
    private readonly IDialogService _dialogService;

    private IFileLoadResultViewModel? _selectedFile;
    private object? _currentView;
    private bool _isSidebarExpanded;
    private bool _isStatusBarVisible = true;
    private IFileLoadResultViewModel? _retryingFile; // File being retried - blocks auto-deselection

    public ReadOnlyObservableCollection<IFileLoadResultViewModel> LoadedFiles => _filesManager.LoadedFiles;
    public bool HasLoadedFiles => LoadedFiles.Count > 0;
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
            // Prevent auto-deselection during file retry to avoid UI flicker
            if (value == null && _retryingFile != null)
            {
                // Don't update - we're in the middle of a retry, keep the old selection visually
                return;
            }

            if (SetField(ref _selectedFile, value))
            {
                // Clear retry flag when new file is selected
                _retryingFile = null;

                // Update FileDetailsViewModel when selection changes
                if (FileDetailsViewModel != null)
                {
                    FileDetailsViewModel.SelectedFile = value;
                }

                // Show/hide File Details tab based on selection
                if (value != null)
                {
                    // File selected - show and switch to File Details tab
                    _tabNavigator.ShowFileDetailsTab();
                }
                else
                {
                    // No file selected - hide File Details tab
                    _tabNavigator.CloseFileDetailsTab();
                }
            }
        }
    }

    public object? CurrentView
    {
        get => _currentView;
        set => SetField(ref _currentView, value);
    }

    // Delegate to TabNavigationCoordinator
    public int SelectedTabIndex
    {
        get => _tabNavigator.SelectedTabIndex;
        set => _tabNavigator.SelectedTabIndex = value;
    }

    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        set => SetField(ref _isSidebarExpanded, value);
    }

    public bool IsFileDetailsTabVisible
    {
        get => _tabNavigator.IsFileDetailsTabVisible;
        set => _tabNavigator.IsFileDetailsTabVisible = value;
    }

    public bool IsSearchTabVisible
    {
        get => _tabNavigator.IsSearchTabVisible;
        set => _tabNavigator.IsSearchTabVisible = value;
    }

    public bool IsComparisonTabVisible
    {
        get => _tabNavigator.IsComparisonTabVisible;
        set => _tabNavigator.IsComparisonTabVisible = value;
    }

    public bool HasAnyTabVisible => _tabNavigator.HasAnyTabVisible;

    public bool IsStatusBarVisible
    {
        get => _isStatusBarVisible;
        set => SetField(ref _isStatusBarVisible, value);
    }

    public IThemeManager ThemeManager { get; }
    public ICommand LoadFileCommand { get; }
    public ICommand UnloadAllFilesCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ToggleSidebarCommand { get; }
    public ICommand ToggleStatusBarCommand { get; }
    public ICommand ShowFileDetailsTabCommand { get; }
    public ICommand ShowSearchTabCommand { get; }
    public ICommand ShowComparisonTabCommand { get; }
    public ICommand CloseFileDetailsTabCommand { get; }
    public ICommand CloseSearchTabCommand { get; }
    public ICommand CloseComparisonTabCommand { get; }
    public ICommand ShowAboutCommand { get; }
    public ICommand ShowDocumentationCommand { get; }
    public ICommand ViewErrorLogCommand { get; }

    // Delegated commands from SearchViewModel
    public ICommand ShowAllFilesCommand => SearchViewModel?.ShowAllFilesCommand ?? new RelayCommand(() => Task.CompletedTask);

    public MainWindowViewModel(
        ILoadedFilesManager filesManager,
        IRowComparisonCoordinator comparisonCoordinator,
        ITabNavigationCoordinator tabNavigator,
        IFileDetailsCoordinator fileDetailsCoordinator,
        IFilePickerService filePickerService,
        ILogService logger,
        IThemeManager themeManager,
        IActivityLogService activityLog,
        IDialogService dialogService)
    {
        _filesManager = filesManager ?? throw new ArgumentNullException(nameof(filesManager));
        _comparisonCoordinator = comparisonCoordinator ?? throw new ArgumentNullException(nameof(comparisonCoordinator));
        _tabNavigator = tabNavigator ?? throw new ArgumentNullException(nameof(tabNavigator));
        _fileDetailsCoordinator = fileDetailsCoordinator ?? throw new ArgumentNullException(nameof(fileDetailsCoordinator));
        _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _activityLog = activityLog ?? throw new ArgumentNullException(nameof(activityLog));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Initialize sidebar as collapsed (new UX)
        _isSidebarExpanded = false;

        LoadFileCommand = new RelayCommand(async () => await LoadFileAsync());

        UnloadAllFilesCommand = new RelayCommand(async () => await UnloadAllFilesAsync());

        ToggleSidebarCommand = new RelayCommand(() =>
        {
            IsSidebarExpanded = !IsSidebarExpanded;
            return Task.CompletedTask;
        });

        ToggleStatusBarCommand = new RelayCommand(() =>
        {
            IsStatusBarVisible = !IsStatusBarVisible;
            return Task.CompletedTask;
        });

        ShowFileDetailsTabCommand = new RelayCommand(() =>
        {
            // Select first file if none selected
            if (SelectedFile == null && LoadedFiles.Any())
            {
                SelectedFile = LoadedFiles.First();
            }
            // File selection will automatically show FileDetails tab
            return Task.CompletedTask;
        });

        ShowSearchTabCommand = new RelayCommand(() =>
        {
            _tabNavigator.ShowSearchTab();
            return Task.CompletedTask;
        });

        ShowComparisonTabCommand = new RelayCommand(() =>
        {
            _tabNavigator.ShowComparisonTab();
            return Task.CompletedTask;
        });

        CloseFileDetailsTabCommand = new RelayCommand(() =>
        {
            _tabNavigator.CloseFileDetailsTab();
            SelectedFile = null;
            return Task.CompletedTask;
        });

        CloseSearchTabCommand = new RelayCommand(() =>
        {
            _tabNavigator.CloseSearchTab();
            return Task.CompletedTask;
        });

        CloseComparisonTabCommand = new RelayCommand(() =>
        {
            _tabNavigator.CloseComparisonTab();
            SelectedComparison = null;
            return Task.CompletedTask;
        });

        ThemeManager = themeManager;
        ToggleThemeCommand = new RelayCommand(() =>
        {
            ThemeManager.ToggleTheme();
            return Task.CompletedTask;
        });

        ShowAboutCommand = new RelayCommand(async () => await ShowAboutDialogAsync());
        ShowDocumentationCommand = new RelayCommand(async () => await OpenDocumentationAsync());
        ViewErrorLogCommand = new RelayCommand(async () => await OpenErrorLogAsync());

        // Subscribe to coordinator events
        _tabNavigator.PropertyChanged += OnTabNavigatorPropertyChanged;

        // Subscribe to file manager events
        _filesManager.FileLoaded += OnFileLoaded;
        _filesManager.FileRemoved += OnFileRemoved;
        _filesManager.FileLoadFailed += OnFileLoadFailed;
        _filesManager.FileReloaded += OnFileReloaded;

        // Subscribe to comparison coordinator events
        _comparisonCoordinator.SelectionChanged += OnComparisonSelectionChanged;
        _comparisonCoordinator.ComparisonRemoved += OnComparisonRemoved;
        _comparisonCoordinator.PropertyChanged += OnComparisonCoordinatorPropertyChanged;
    }

    private void OnTabNavigatorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Propagate PropertyChanged from TabNavigationCoordinator to ViewModel
        // so XAML bindings are updated
        if (e.PropertyName == nameof(ITabNavigationCoordinator.SelectedTabIndex))
        {
            OnPropertyChanged(nameof(SelectedTabIndex));
        }
        else if (e.PropertyName == nameof(ITabNavigationCoordinator.IsFileDetailsTabVisible))
        {
            OnPropertyChanged(nameof(IsFileDetailsTabVisible));
        }
        else if (e.PropertyName == nameof(ITabNavigationCoordinator.IsSearchTabVisible))
        {
            OnPropertyChanged(nameof(IsSearchTabVisible));
        }
        else if (e.PropertyName == nameof(ITabNavigationCoordinator.IsComparisonTabVisible))
        {
            OnPropertyChanged(nameof(IsComparisonTabVisible));
        }
        else if (e.PropertyName == nameof(ITabNavigationCoordinator.HasAnyTabVisible))
        {
            OnPropertyChanged(nameof(HasAnyTabVisible));
        }
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

        // If Search tab is visible, switch to it; otherwise just deselect
        if (IsSearchTabVisible)
        {
            _tabNavigator.ShowSearchTab();
        }
        else
        {
            SelectedTabIndex = -1;
        }

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
            SearchViewModel.PropertyChanged += OnSearchViewModelPropertyChanged;
        }
    }

    private void OnSearchViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchViewModel.SearchResults) && TreeSearchResultsViewModel != null)
        {
            var query = SearchViewModel.SearchQuery;
            var results = SearchViewModel.SearchResults;
            if (!string.IsNullOrWhiteSpace(query) && results?.Any() == true)
            {
                TreeSearchResultsViewModel.AddSearchResults(query, results.ToList());

                // Show and switch to Search tab to display results
                _tabNavigator.ShowSearchTab();
            }
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
        // Show/hide comparison tab based on selection
        if (e.NewSelection != null)
        {
            // Comparison created/selected - show and switch to Comparison tab
            _tabNavigator.ShowComparisonTab();
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
        {
            return;
        }

        // Ask for confirmation
        var confirmed = await _dialogService.ShowConfirmationAsync(
            $"Are you sure you want to unload all {LoadedFiles.Count} file(s)?\n\n" +
            "This will clear all data, search results, and comparisons.",
            "Unload All Files"
        );

        if (!confirmed)
        {
            return;
        }

        _activityLog.LogInfo($"Unloading all {LoadedFiles.Count} file(s)...", "FileUnload");

        // Clear selection first
        SelectedFile = null;

        // Clear all comparisons first
        var comparisonsToRemove = RowComparisons.ToList();
        foreach (var comparison in comparisonsToRemove)
        {
            _comparisonCoordinator.RemoveComparison(comparison);
        }

        // Clear all search results
        TreeSearchResultsViewModel?.ClearHistory();
        SearchViewModel?.ClearSearchCommand.Execute(null);

        // Remove all files (iterate backwards to avoid collection modification issues)
        var filesToRemove = LoadedFiles.ToList();
        foreach (var file in filesToRemove)
        {
            file.Dispose();
            _filesManager.RemoveFile(file);
        }

        _activityLog.LogInfo("All files unloaded successfully", "FileUnload");
        _logger.LogInfo($"Unloaded {filesToRemove.Count} file(s)", "MainWindowViewModel");
    }

    // Event handlers for FileDetailsViewModel - delegate to FileDetailsCoordinator
    private void OnRemoveFromListRequested(IFileLoadResultViewModel? file) =>
        _fileDetailsCoordinator.HandleRemoveFromList(file);

    private void OnCleanAllDataRequested(IFileLoadResultViewModel? file) =>
        _fileDetailsCoordinator.HandleCleanAllData(
            file,
            TreeSearchResultsViewModel,
            SearchViewModel,
            fileToCheck =>
            {
                // Clear selection if this file is currently selected (prevent memory leak)
                if (SelectedFile == fileToCheck)
                {
                    SelectedFile = null;
                }
            });

    private void OnRemoveNotificationRequested(IFileLoadResultViewModel? file) =>
        _fileDetailsCoordinator.HandleRemoveNotification(file);

    private void OnTryAgainRequested(IFileLoadResultViewModel? file)
    {
        if (file == null)
            return;

        // CRITICAL: Set retry flag BEFORE calling HandleTryAgainAsync
        // This prevents UI flicker during file removal/reload cycle
        // Must happen before RemoveFile is called to block Avalonia's auto-deselection
        _retryingFile = file;
        _logger.LogInfo($"Starting retry for: {file.FileName}, preserving selection", "MainWindowViewModel");

        // Use fire-and-forget pattern
        // The FileReloaded event will automatically update SelectedFile when reload completes (event-driven)
        _ = _fileDetailsCoordinator.HandleTryAgainAsync(file, _ => { /* Event-driven: OnFileReloaded handles update */ });
    }

    // Event handlers for FilesManager events
    private void OnFileLoaded(object? sender, FileLoadedEventArgs e)
    {
        _logger.LogInfo($"File loaded: {e.File.FileName} (HasErrors: {e.HasErrors})", "MainWindowViewModel");

        // Notify that HasLoadedFiles changed
        OnPropertyChanged(nameof(HasLoadedFiles));

        // Auto-open sidebar when first file is loaded
        if (LoadedFiles.Count == 1)
        {
            IsSidebarExpanded = true;
        }
    }

    private void OnFileRemoved(object? sender, FileRemovedEventArgs e)
    {
        _logger.LogInfo($"File removed: {e.File.FileName} (IsRetry: {e.IsRetry})", "MainWindowViewModel");

        // Notify that HasLoadedFiles changed
        OnPropertyChanged(nameof(HasLoadedFiles));

        // Auto-close sidebar when last file is removed (but not during retry)
        if (LoadedFiles.Count == 0 && !e.IsRetry)
        {
            IsSidebarExpanded = false;
        }
    }

    private void OnFileLoadFailed(object? sender, FileLoadFailedEventArgs e)
    {
        _logger.LogError($"File load failed: {e.FilePath}", e.Exception, "MainWindowViewModel");
    }

    // Event handler for file reload events (event-driven architecture)
    private void OnFileReloaded(object? sender, FileReloadedEventArgs e)
    {
        _logger.LogInfo($"OnFileReloaded event received for: {e.NewFile.FileName}", "MainWindowViewModel");

        // If we're retrying the currently selected file, update SelectedFile to the new instance
        // This triggers FileDetailsViewModel update automatically via the master-slave pattern
        if (_retryingFile != null && _retryingFile.FilePath.Equals(e.FilePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInfo($"Updating SelectedFile to reloaded instance: {e.NewFile.FileName}", "MainWindowViewModel");

            // Temporarily clear retry flag to allow the update, then set new file
            _retryingFile = null;
            SelectedFile = e.NewFile; // This propagates to FileDetailsViewModel automatically
        }
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            _filesManager.FileLoaded -= OnFileLoaded;
            _filesManager.FileRemoved -= OnFileRemoved;
            _filesManager.FileLoadFailed -= OnFileLoadFailed;
            _filesManager.FileReloaded -= OnFileReloaded;

            _comparisonCoordinator.SelectionChanged -= OnComparisonSelectionChanged;
            _comparisonCoordinator.ComparisonRemoved -= OnComparisonRemoved;
            _comparisonCoordinator.PropertyChanged -= OnComparisonCoordinatorPropertyChanged;

            if (TreeSearchResultsViewModel != null)
            {
                TreeSearchResultsViewModel.RowComparisonCreated -= OnRowComparisonCreated;
                TreeSearchResultsViewModel.Dispose();
            }

            if (FileDetailsViewModel != null)
            {
                FileDetailsViewModel.RemoveFromListRequested -= OnRemoveFromListRequested;
                FileDetailsViewModel.CleanAllDataRequested -= OnCleanAllDataRequested;
                FileDetailsViewModel.RemoveNotificationRequested -= OnRemoveNotificationRequested;
                FileDetailsViewModel.TryAgainRequested -= OnTryAgainRequested;
            }

            if (SearchViewModel != null)
            {
                SearchViewModel.PropertyChanged -= OnSearchViewModelPropertyChanged;
                SearchViewModel.Dispose();
            }

            FileDetailsViewModel = null;
        }

        _disposed = true;
    }
}

