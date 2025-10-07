using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ExcelViewer.UI.Avalonia.Views;
using ExcelViewer.UI.Avalonia.ViewModels;
using ExcelViewer.UI.Avalonia.Services;
using ExcelViewer.UI.Avalonia.Managers.Search;
using ExcelViewer.UI.Avalonia.Managers.Selection;
using ExcelViewer.UI.Avalonia.Managers.Files;
using ExcelViewer.UI.Avalonia.Managers.Comparison;
using ExcelViewer.UI.Avalonia.Models.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ExcelViewer.Core.Application.Services;
using ExcelViewer.Core.Application.Interfaces;
using ExcelViewer.Infrastructure.External;
using ExcelViewer.Infrastructure.External.Readers;
using ExcelViewer.UI.Avalonia.Managers;

namespace ExcelViewer.UI.Avalonia;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Create and configure the host
        _host = CreateHostBuilder().Build();

        // Initialize theme manager
        var themeManager = _host.Services.GetRequiredService<IThemeManager>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Get the main window and view model from DI container
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            var mainViewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
            var searchViewModel = _host.Services.GetRequiredService<SearchViewModel>();
            var fileDetailsViewModel = _host.Services.GetRequiredService<FileDetailsViewModel>();
            var treeSearchResultsViewModel = _host.Services.GetRequiredService<TreeSearchResultsViewModel>();

            // Wire up ViewModels to MainViewModel
            mainViewModel.SetSearchViewModel(searchViewModel);
            mainViewModel.SetFileDetailsViewModel(fileDetailsViewModel);
            mainViewModel.SetTreeSearchResultsViewModel(treeSearchResultsViewModel);

            mainWindow.DataContext = mainViewModel;
            desktop.MainWindow = mainWindow;

            // Handle application exit
            desktop.Exit += (_, _) => _host?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register Core services
                services.AddSingleton<ICellReferenceParser, CellReferenceParser>();
                services.AddSingleton<ICellValueReader, CellValueReader>();
                services.AddSingleton<IMergedCellProcessor, MergedCellProcessor>();

                // Register file format readers (must be before ExcelReaderService)
                services.AddSingleton<IFileFormatReader, OpenXmlFileReader>();
                services.AddSingleton<IFileFormatReader, XlsFileReader>();
                services.AddSingleton<IFileFormatReader, CsvFileReader>();

                services.AddSingleton<IExcelReaderService, ExcelReaderService>();
                services.AddSingleton<ISearchService, SearchService>();
                services.AddSingleton<IRowComparisonService, RowComparisonService>();
                services.AddSingleton<IExceptionHandler, ExceptionHandler>();

                // Register Avalonia-specific services
                services.AddSingleton<IDialogService, AvaloniaDialogService>();
                services.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
                services.AddSingleton<IErrorNotificationService, ErrorNotificationService>();
                services.AddSingleton<IActivityLogService, ActivityLogService>();


                // Register Managers and Factories
                services.AddSingleton<ISearchResultFactory, ExcelViewer.UI.Avalonia.Models.Search.SearchResultFactory>();
                services.AddSingleton<ISearchResultsManager, ExcelViewer.UI.Avalonia.Managers.Search.SearchResultsManager>();
                services.AddSingleton<ISelectionManager, ExcelViewer.UI.Avalonia.Managers.Selection.SelectionManager>();
                services.AddSingleton<IThemeManager, ThemeManager>();
                services.AddSingleton<ILoadedFilesManager, LoadedFilesManager>();
                services.AddSingleton<IRowComparisonCoordinator, RowComparisonCoordinator>();

                // Register ViewModels
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<SearchViewModel>();
                services.AddSingleton<FileDetailsViewModel>();
                services.AddSingleton<TreeSearchResultsViewModel>();

                // Register Views
                services.AddSingleton<MainWindow>();

                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
    }

}
