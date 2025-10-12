using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SheetAtlas.UI.Avalonia.ViewModels;

namespace SheetAtlas.UI.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnHeaderTapped(object? sender, TappedEventArgs e)
    {
        // Clear selection when tapping header area
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectedFile = null;
        }
    }

    private void OnClearSelectionClick(object? sender, RoutedEventArgs e)
    {
        // Clear selection when clicking X button
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectedFile = null;
        }
    }

    private void OnSearchTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        // Trigger search when Enter key is pressed
        if (e.Key == Key.Enter && DataContext is MainWindowViewModel viewModel)
        {
            if (viewModel.SearchViewModel?.SearchCommand?.CanExecute(null) == true)
            {
                viewModel.SearchViewModel.SearchCommand.Execute(null);
            }
        }
    }
}
