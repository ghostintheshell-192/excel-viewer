using System.ComponentModel;

namespace SheetAtlas.UI.Avalonia.Managers
{
    public enum Theme
    {
        Light,
        Dark
    }

    public interface IThemeManager : INotifyPropertyChanged
    {
        Theme CurrentTheme { get; }

        // Theme properties
        public string ThemeButtonText { get; }
        public string ThemeButtonTooltip { get; }
        void SetTheme(Theme theme);
        void ToggleTheme();
        event EventHandler<Theme>? ThemeChanged;


    }
}
