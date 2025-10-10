namespace SheetAtlas.UI.Avalonia.Services;

public interface IDialogService
{
    Task ShowMessageAsync(string message, string title = "Information");
    Task ShowErrorAsync(string message, string title = "Error");
    Task ShowWarningAsync(string message, string title = "Warning");
    Task ShowInformationAsync(string message, string title = "Information");
    Task<bool> ShowConfirmationAsync(string message, string title = "Confirmation");
}
