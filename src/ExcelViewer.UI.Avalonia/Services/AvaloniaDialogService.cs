using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;

namespace ExcelViewer.UI.Avalonia.Services;

public class AvaloniaDialogService : IDialogService
{
    private Window? GetMainWindow()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    public async Task ShowMessageAsync(string message, string title = "Information")
    {
        await ShowDialogAsync(title, message);
    }

    public async Task ShowErrorAsync(string message, string title = "Error")
    {
        await ShowDialogAsync(title, message);
    }

    public async Task ShowWarningAsync(string message, string title = "Warning")
    {
        await ShowDialogAsync(title, message);
    }

    public async Task ShowInformationAsync(string message, string title = "Information")
    {
        await ShowDialogAsync(title, message);
    }

    private async Task ShowDialogAsync(string title, string message)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            var messageBox = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            stackPanel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });

            var button = new Button
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            button.Click += (_, _) => messageBox.Close();
            stackPanel.Children.Add(button);

            messageBox.Content = stackPanel;
            await messageBox.ShowDialog(mainWindow);
        }
    }

    public async Task<bool> ShowConfirmationAsync(string message, string title = "Confirmation")
    {
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            var messageBox = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            stackPanel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            bool result = false;

            var yesButton = new Button
            {
                Content = "Yes",
                Margin = new Thickness(0, 0, 10, 0)
            };
            yesButton.Click += (_, _) => { result = true; messageBox.Close(); };

            var noButton = new Button
            {
                Content = "No"
            };
            noButton.Click += (_, _) => { result = false; messageBox.Close(); };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
            stackPanel.Children.Add(buttonPanel);

            messageBox.Content = stackPanel;
            await messageBox.ShowDialog(mainWindow);
            return result;
        }

        return false;
    }
}
