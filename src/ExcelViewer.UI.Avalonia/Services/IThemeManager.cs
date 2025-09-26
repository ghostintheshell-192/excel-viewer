using System.ComponentModel;

namespace ExcelViewer.UI.Avalonia.Services
{
    public enum Theme
    {
        Light,
        Dark
    }

    public interface IThemeManager : INotifyPropertyChanged
    {
        Theme CurrentTheme { get; }
        void SetTheme(Theme theme);
        void ToggleTheme();
        event EventHandler<Theme>? ThemeChanged;
    }
}