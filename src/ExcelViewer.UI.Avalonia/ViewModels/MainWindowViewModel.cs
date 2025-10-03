using System.Collections.ObjectModel;
using System.Windows.Input;
using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.Core.Domain.ValueObjects;
using ExcelViewer.Core.Application.DTOs;
using ExcelViewer.Infrastructure.External;
using ExcelViewer.UI.Avalonia.Models.Search;
using ExcelViewer.UI.Avalonia.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ExcelViewer.UI.Avalonia.Commands;
using ExcelViewer.UI.Avalonia.Managers;
using ExcelViewer.UI.Avalonia.Managers.Files;
using ExcelViewer.UI.Avalonia.Managers.Comparison;

namespace ExcelViewer.UI.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ILoadedFilesManager _filesManager;
    private readonly IRowComparisonCoordinator _comparisonCoordinator;
    private readonly IFilePickerService _filePickerService;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IThemeManager _themeManager;

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

    // Delegated commands from SearchViewModel
    public ICommand ShowAllFilesCommand => SearchViewModel?.ShowAllFilesCommand ?? new RelayCommand(() => Task.CompletedTask);

    public MainWindowViewModel(
        ILoadedFilesManager filesManager,
        IRowComparisonCoordinator comparisonCoordinator,
        IFilePickerService filePickerService,
        ILogger<MainWindowViewModel> logger,
        IThemeManager themeManager)
    {
        _filesManager = filesManager ?? throw new ArgumentNullException(nameof(filesManager));
        _comparisonCoordinator = comparisonCoordinator ?? throw new ArgumentNullException(nameof(comparisonCoordinator));
        _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));

        // Initialize with Search Results tab selected
        _selectedTabIndex = 1;

        LoadFileCommand = new RelayCommand(async () => await LoadFileAsync());

        ThemeManager = themeManager;
        ToggleThemeCommand = new RelayCommand(() =>
        {
            ThemeManager.ToggleTheme();
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
        var files = await _filePickerService.OpenFilesAsync("Select Excel Files", new[] { "*.xlsx", "*.xls" });
        if (files?.Any() == true)
        {
            await _filesManager.LoadFilesAsync(files);
        }
    }

    // Event handlers for FileDetailsViewModel - delegate to FilesManager
    private void OnRemoveFromListRequested(IFileLoadResultViewModel? file) => _filesManager.RemoveFile(file);

    private void OnCleanAllDataRequested(IFileLoadResultViewModel? file)
    {
        // TODO: Implement search results cleanup for this file
        _logger.LogInformation("Clean all data requested for: {FileName}", file?.FileName);
        _filesManager.RemoveFile(file);
    }

    private void OnRemoveNotificationRequested(IFileLoadResultViewModel? file) => _filesManager.RemoveFile(file);

    private async void OnTryAgainRequested(IFileLoadResultViewModel? file)
    {
        if (file != null)
        {
            await _filesManager.RetryLoadAsync(file.FilePath);
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

