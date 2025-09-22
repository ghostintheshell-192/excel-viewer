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

namespace ExcelViewer.UI.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IExcelReaderService _excelReaderService;
    private readonly IFilePickerService _filePickerService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;

    private IFileLoadResultViewModel? _selectedFile;
    private object? _currentView;
    private int _selectedTabIndex;

    private readonly ObservableCollection<IFileLoadResultViewModel> _loadedFiles = new();
    public ReadOnlyObservableCollection<IFileLoadResultViewModel> LoadedFiles { get; }

    public SearchViewModel? SearchViewModel { get; private set; }
    public FileDetailsViewModel? FileDetailsViewModel { get; private set; }
    public TreeSearchResultsViewModel? TreeSearchResultsViewModel { get; private set; }

    // Row comparison management
    private readonly ObservableCollection<RowComparisonViewModel> _rowComparisons = new();
    public ReadOnlyObservableCollection<RowComparisonViewModel> RowComparisons { get; }

    private RowComparisonViewModel? _selectedComparison;
    public RowComparisonViewModel? SelectedComparison
    {
        get => _selectedComparison;
        set
        {
            if (SetField(ref _selectedComparison, value))
            {
                // Switch to comparison tab when a comparison is selected
                if (value != null)
                {
                    SelectedTabIndex = 2; // Comparison tab will be index 2
                }
            }
        }
    }


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

    public ICommand LoadFileCommand { get; }

    // Delegated commands from SearchViewModel
    public ICommand ShowAllFilesCommand => SearchViewModel?.ShowAllFilesCommand ?? new RelayCommand(() => Task.CompletedTask);

    public MainWindowViewModel(
        IExcelReaderService excelReaderService,
        IFilePickerService filePickerService,
        IDialogService dialogService,
        ILogger<MainWindowViewModel> logger,
        IServiceProvider serviceProvider)
    {
        _excelReaderService = excelReaderService ?? throw new ArgumentNullException(nameof(excelReaderService));
        _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        LoadedFiles = new ReadOnlyObservableCollection<IFileLoadResultViewModel>(_loadedFiles);
        RowComparisons = new ReadOnlyObservableCollection<RowComparisonViewModel>(_rowComparisons);

        // Initialize with Search Results tab selected
        _selectedTabIndex = 1;

        LoadFileCommand = new RelayCommand(async () => await LoadFileAsync());

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
        try
        {
            var comparisonLogger = _serviceProvider.GetRequiredService<ILogger<RowComparisonViewModel>>();
            var comparisonViewModel = new RowComparisonViewModel(comparison, comparisonLogger);

            // Handle close request
            comparisonViewModel.CloseRequested += (s, e) =>
            {
                var vm = s as RowComparisonViewModel;
                if (vm != null && _rowComparisons.Contains(vm))
                {
                    _rowComparisons.Remove(vm);
                    _logger.LogInformation("Removed row comparison: {ComparisonName}", vm.Title);

                    // If this was the selected comparison, clear selection
                    if (SelectedComparison == vm)
                    {
                        SelectedComparison = null;
                    }
                }
            };

            _rowComparisons.Add(comparisonViewModel);
            SelectedComparison = comparisonViewModel; // Auto-select the new comparison

            _logger.LogInformation("Added new row comparison: {ComparisonName} with {RowCount} rows",
                comparison.Name, comparison.Rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create row comparison view model");
        }
    }





    private async Task LoadFileAsync()
    {
        try
        {
            var files = await _filePickerService.OpenFilesAsync("Select Excel Files", new[] { "*.xlsx", "*.xls" });
            if (files?.Any() == true)
            {
                _logger.LogInformation("Loading {FileCount} files", files.Count());

                var loadedExcelFiles = await _excelReaderService.LoadFilesAsync(files);

                foreach (var excelFile in loadedExcelFiles)
                {
                    // Check for duplicates
                    if (LoadedFiles.Any(f => f.FilePath.Equals(excelFile.FilePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        await _dialogService.ShowMessageAsync(
                            $"File {excelFile.FileName} is already loaded.",
                            "Duplicate File");
                        continue;
                    }

                    _loadedFiles.Add(new FileLoadResultViewModel(excelFile));

                    if (excelFile.HasErrors)
                    {
                        _logger.LogWarning("File {FilePath} loaded with errors", excelFile.FilePath);
                    }
                }

                _logger.LogInformation("Successfully loaded {FileCount} files", loadedExcelFiles.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading files");
            await _dialogService.ShowErrorAsync(
                $"Error loading files: {ex.Message}",
                "Load Error");
        }
    }


    private void RemoveFile(IFileLoadResultViewModel? file)
    {
        if (file != null && _loadedFiles.Contains(file))
        {
            _loadedFiles.Remove(file);
            _logger.LogInformation("Removed file: {FileName}", file.FileName);
        }
    }

    // Event handlers for FileDetailsViewModel
    private void OnRemoveFromListRequested(IFileLoadResultViewModel? file)
    {
        RemoveFile(file);
    }

    private void OnCleanAllDataRequested(IFileLoadResultViewModel? file)
    {
        if (file != null)
        {
            // TODO: Clean search results for this file
            // TODO: Clean any cached data for this file
            _logger.LogInformation("Cleaning all data for file: {FileName}", file.FileName);

            RemoveFile(file);
        }
    }

    private void OnRemoveNotificationRequested(IFileLoadResultViewModel? file)
    {
        // For failed files, just remove from list (no data to clean)
        RemoveFile(file);
    }

    private async void OnTryAgainRequested(IFileLoadResultViewModel? file)
    {
        if (file != null)
        {
            try
            {
                _logger.LogInformation("Retrying file load for: {FileName}", file.FileName);

                // Remove the failed file first
                RemoveFile(file);

                // Attempt to reload
                var reloadedFiles = await _excelReaderService.LoadFilesAsync(new[] { file.FilePath });

                foreach (var reloadedFile in reloadedFiles)
                {
                    _loadedFiles.Add(new FileLoadResultViewModel(reloadedFile));

                    if (reloadedFile.HasErrors)
                    {
                        _logger.LogWarning("File {FilePath} reloaded with errors", reloadedFile.FilePath);
                    }
                    else
                    {
                        _logger.LogInformation("File {FilePath} reloaded successfully", reloadedFile.FilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying file load for: {FileName}", file.FileName);
                await _dialogService.ShowErrorAsync($"Failed to reload file: {ex.Message}", "Reload Error");
            }
        }
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

// Simple RelayCommand implementation
public class RelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            await _execute();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;

    public void Execute(object? parameter) => _execute((T)parameter!);

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}