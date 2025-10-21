using System.Collections.ObjectModel;
using SheetAtlas.UI.Avalonia.Services;
using SheetAtlas.Logging.Services;
using SheetAtlas.UI.Avalonia.Managers;
using SheetAtlas.UI.Avalonia.Managers.Files;
using SheetAtlas.UI.Avalonia.Managers.Comparison;

namespace SheetAtlas.UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
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
    private bool _isFileDetailsTabVisible;
    private bool _isSearchTabVisible;
    private bool _isComparisonTabVisible;
    private bool _isStatusBarVisible = true;
    private bool _disposed = false;

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

                // Show/hide File Details tab based on selection
                if (value != null)
                {
                    // File selected - show and switch to File Details tab
                    IsFileDetailsTabVisible = true;
                    SelectedTabIndex = GetTabIndex("FileDetails");
                }
                else
                {
                    // No file selected - hide File Details tab
                    IsFileDetailsTabVisible = false;
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

    public bool IsFileDetailsTabVisible
    {
        get => _isFileDetailsTabVisible;
        set
        {
            if (SetField(ref _isFileDetailsTabVisible, value))
            {
                OnPropertyChanged(nameof(HasAnyTabVisible));
            }
        }
    }

    public bool IsSearchTabVisible
    {
        get => _isSearchTabVisible;
        set
        {
            if (SetField(ref _isSearchTabVisible, value))
            {
                OnPropertyChanged(nameof(HasAnyTabVisible));
            }
        }
    }

    public bool IsComparisonTabVisible
    {
        get => _isComparisonTabVisible;
        set
        {
            if (SetField(ref _isComparisonTabVisible, value))
            {
                OnPropertyChanged(nameof(HasAnyTabVisible));
            }
        }
    }

    public bool HasAnyTabVisible => IsFileDetailsTabVisible || IsSearchTabVisible || IsComparisonTabVisible;

    public bool IsStatusBarVisible
    {
        get => _isStatusBarVisible;
        set => SetField(ref _isStatusBarVisible, value);
    }

    public IThemeManager ThemeManager { get; }
    // public ICommand ShowAllFilesCommand => SearchViewModel?.ShowAllFilesCommand ?? new RelayCommand(() => Task.CompletedTask);

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

        ThemeManager = themeManager;

        // Initialize with no tab selected (clean start)
        _selectedTabIndex = -1;

        _isSidebarExpanded = false;
        _isFileDetailsTabVisible = false;
        _isSearchTabVisible = false;
        _isComparisonTabVisible = false;

        InitializeCommands();
        SubscribeToEvents();

    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }

    protected void Dispose(bool v)
    {
        if (v)
        {
            UnsubscribeFromEvents();
            _filesManager.Dispose();
            _comparisonCoordinator.Dispose();
            SearchViewModel?.Dispose();
            // FileDetailsViewModel?.Dispose();
            TreeSearchResultsViewModel?.Dispose();
        }
    }
}

