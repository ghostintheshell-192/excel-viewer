using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ExcelViewer.UI.Avalonia.Views;
using ExcelViewer.UI.Avalonia.ViewModels;
using ExcelViewer.UI.Avalonia.Services;
using ExcelViewer.UI.Avalonia.Managers.Search;
using ExcelViewer.UI.Avalonia.Managers.Selection;
using ExcelViewer.UI.Avalonia.Models.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ExcelViewer.Core.Application.Services;
using ExcelViewer.Core.Application.Interfaces;
using ExcelViewer.Infrastructure.External;

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
                services.AddScoped<ICellReferenceParser, CellReferenceParser>();
                services.AddScoped<IMergedCellProcessor, MergedCellProcessor>();
                services.AddScoped<IExcelReaderService, ExcelReaderService>();
                services.AddScoped<ISearchService, SearchService>();
                services.AddScoped<IRowComparisonService, RowComparisonService>();

                // Register Avalonia-specific services
                services.AddScoped<IDialogService, AvaloniaDialogService>();
                services.AddScoped<IFilePickerService, AvaloniaFilePickerService>();
                services.AddSingleton<IThemeManager, ThemeManager>();

                // Register Managers and Factories
                services.AddScoped<ISearchResultFactory, ExcelViewer.UI.Avalonia.Models.Search.SearchResultFactory>();
                services.AddScoped<ISearchResultsManager, ExcelViewer.UI.Avalonia.Managers.Search.SearchResultsManager>();
                services.AddScoped<ISelectionManager, ExcelViewer.UI.Avalonia.Managers.Selection.SelectionManager>();

                // Register ViewModels
                services.AddScoped<MainWindowViewModel>();
                services.AddScoped<SearchViewModel>();
                services.AddScoped<FileDetailsViewModel>();
                services.AddScoped<TreeSearchResultsViewModel>();

                // Register Views
                services.AddScoped<MainWindow>();

                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
    }

}