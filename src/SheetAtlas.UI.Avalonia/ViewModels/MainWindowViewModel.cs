using System.Collections.ObjectModel;
using System.Windows.Input;
using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Core.Application.DTOs;
using SheetAtlas.Infrastructure.External;
using SheetAtlas.UI.Avalonia.Models.Search;
using SheetAtlas.UI.Avalonia.Services;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IThemeManager _themeManager;
    private readonly IActivityLogService _activityLog;
    private readonly IDialogService _dialogService;

    private IFileLoadResultViewModel? _selectedFile;
    private object? _currentView;
    private int _selectedTabIndex;

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

    public IThemeManager ThemeManager { get; }
    public ICommand LoadFileCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ForceGCCommand { get; } // TEMPORARY: for memory leak testing

    // Delegated commands from SearchViewModel
    public ICommand ShowAllFilesCommand => SearchViewModel?.ShowAllFilesCommand ?? new RelayCommand(() => Task.CompletedTask);

    public MainWindowViewModel(
        ILoadedFilesManager filesManager,
        IRowComparisonCoordinator comparisonCoordinator,
        IFilePickerService filePickerService,
        ILogger<MainWindowViewModel> logger,
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

        LoadFileCommand = new RelayCommand(async () => await LoadFileAsync());

        ThemeManager = themeManager;
        ToggleThemeCommand = new RelayCommand(() =>
        {
            ThemeManager.ToggleTheme();
            return Task.CompletedTask;
        });

        // TEMPORARY: for memory leak testing
        ForceGCCommand = new RelayCommand(() =>
        {
            _logger.LogInformation("Force GC requested by user");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            _activityLog.LogInfo("Garbage collection forzata completata", "Memory");
            return Task.CompletedTask;
        });

        // Subscribe to file manager events
        _filesManager.FileLoaded += OnFileLoaded;
        _filesManager.FileRemoved += OnFileRemoved;
        _filesManager.FileLoadFailed += OnFileLoadFailed;

        // Subscribe to comparison coordinator events
        _comparisonCoordinator.SelectionChanged += OnComparisonSelectionChanged;
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
        FileDetailsViewModel.ViewSheetsRequested += OnViewSheetsRequested;

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
        // Switch to comparison tab when a comparison is selected
        if (e.NewSelection != null)
        {
            SelectedTabIndex = 2; // Comparison tab
        }
    }

    private async Task LoadFileAsync()
    {
        try
        {
            _activityLog.LogInfo("Apertura selezione file...", "FileLoad");

            var files = await _filePickerService.OpenFilesAsync("Select Excel Files", new[] { "*.xlsx", "*.xls" });

            if (files?.Any() != true)
            {
                // User cancelled or didn't select any files - this is normal
                _activityLog.LogInfo("Selezione file annullata dall'utente", "FileLoad");
                return;
            }

            _activityLog.LogInfo($"Caricamento di {files.Count()} file...", "FileLoad");
            await _filesManager.LoadFilesAsync(files);

            _activityLog.LogInfo($"Caricamento completato: {files.Count()} file", "FileLoad");
        }
        catch (Exception ex)
        {
            // Safety net for unexpected errors
            // Note: FilePickerService and ExcelReaderService handle their own errors internally
            // This catch is only for truly unexpected issues (OOM, async state corruption, etc.)
            _logger.LogError(ex, "Unexpected error when loading files");
            _activityLog.LogError("Errore imprevisto durante il caricamento", ex, "FileLoad");

            await _dialogService.ShowErrorAsync(
                "Si è verificato un errore imprevisto durante il caricamento dei file.\n\n" +
                $"Dettaglio: {ex.Message}\n\n" +
                "L'operazione è stata annullata.",
                "Errore Caricamento"
            );
        }
    }

    // Event handlers for FileDetailsViewModel - delegate to FilesManager
    private void OnRemoveFromListRequested(IFileLoadResultViewModel? file) => _filesManager.RemoveFile(file);

    private void OnCleanAllDataRequested(IFileLoadResultViewModel? file)
    {
        if (file == null)
        {
            _logger.LogWarning("Clean all data requested with null file");
            return;
        }

        _logger.LogInformation("Clean all data requested for: {FileName}", file.FileName);

        // Clear selection if this file is currently selected (prevent memory leak)
        if (SelectedFile == file)
        {
            SelectedFile = null;
        }

        // Remove search results that reference this file
        TreeSearchResultsViewModel?.RemoveSearchResultsForFile(file.File);

        // Remove row comparisons that reference this file
        _comparisonCoordinator.RemoveComparisonsForFile(file.File);

        // Finally, remove the file from the loaded files list
        _filesManager.RemoveFile(file);

        _logger.LogInformation("Cleaned all data for file: {FileName}", file.FileName);
    }

    private void OnRemoveNotificationRequested(IFileLoadResultViewModel? file) => _filesManager.RemoveFile(file);

    private void OnTryAgainRequested(IFileLoadResultViewModel? file)
    {
        if (file == null)
        {
            _logger.LogWarning("Try again requested but file is null");
            return;
        }

        // Use fire-and-forget pattern with proper error handling
        _ = RetryLoadFileAsync(file);
    }

    private async Task RetryLoadFileAsync(IFileLoadResultViewModel file)
    {
        try
        {
            _activityLog.LogInfo($"Nuovo tentativo di caricamento: {file.FileName}", "FileRetry");
            _logger.LogInformation("Retrying file load for: {FilePath}", file.FilePath);

            await _filesManager.RetryLoadAsync(file.FilePath);

            _activityLog.LogInfo($"Tentativo completato: {file.FileName}", "FileRetry");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying file load: {FilePath}", file.FilePath);
            _activityLog.LogError($"Errore nel ricaricamento di {file.FileName}", ex, "FileRetry");

            await _dialogService.ShowErrorAsync(
                $"Impossibile ricaricare il file '{file.FileName}'.\n\n" +
                $"Dettaglio: {ex.Message}",
                "Errore Ricaricamento"
            );
        }
    }

    // Event handlers for FilesManager events
    private void OnFileLoaded(object? sender, FileLoadedEventArgs e)
    {
        _logger.LogInformation("File loaded: {FileName} (HasErrors: {HasErrors})",
            e.File.FileName, e.HasErrors);
    }

    private void OnFileRemoved(object? sender, FileRemovedEventArgs e)
    {
        _logger.LogInformation("File removed: {FileName}", e.File.FileName);
    }

    private void OnFileLoadFailed(object? sender, FileLoadFailedEventArgs e)
    {
        _logger.LogError(e.Exception, "File load failed: {FilePath}", e.FilePath);
    }

    private void OnViewSheetsRequested(IFileLoadResultViewModel? file)
    {
        if (file != null)
        {
            _logger.LogInformation("View sheets requested for: {FileName}", file.FileName);
            // TODO: Navigate to sheet view or show sheet details
            // This could open a new window or change the current view
        }
    }
}

