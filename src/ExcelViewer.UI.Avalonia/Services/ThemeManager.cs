using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.UI.Avalonia.Services
{
    public class ThemeManager : IThemeManager
    {
        private readonly ILogger<ThemeManager> _logger;
        private Theme _currentTheme = Theme.Light;

        public Theme CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTheme)));
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Theme>? ThemeChanged;

        public ThemeManager(ILogger<ThemeManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize with system preference if available
            InitializeTheme();
        }

        public void SetTheme(Theme theme)
        {
            _logger.LogInformation("Setting theme to {Theme}", theme);

            try
            {
                ApplyTheme(theme);
                CurrentTheme = theme;
                SaveThemePreference(theme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set theme to {Theme}", theme);
                throw;
            }
        }

        public void ToggleTheme()
        {
            var newTheme = CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
            SetTheme(newTheme);
        }

        private void InitializeTheme()
        {
            try
            {
                // Load saved preference or default to Light
                var savedTheme = LoadThemePreference();
                ApplyTheme(savedTheme);
                CurrentTheme = savedTheme;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize theme, using Light theme as fallback");
                ApplyTheme(Theme.Light);
                CurrentTheme = Theme.Light;
            }
        }

        private void ApplyTheme(Theme theme)
        {
            var application = Application.Current;
            if (application == null)
            {
                _logger.LogWarning("Application.Current is null, cannot apply theme");
                return;
            }

            try
            {
                var themeKey = theme == Theme.Light ? "LightTheme" : "DarkTheme";

                // Find the theme resources file
                var themeResourcesDict = application.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.TryGetResource(themeKey, null, out _));

                if (themeResourcesDict != null &&
                    themeResourcesDict.TryGetResource(themeKey, null, out var themeDict) &&
                    themeDict is ResourceDictionary themeDictionary)
                {
                    // Clear existing theme resources and apply new ones
                    var keysToRemove = application.Resources.Keys
                        .Where(key => IsThemeResource(key))
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        application.Resources.Remove(key);
                    }

                    // Apply new theme resources
                    foreach (var kvp in themeDictionary)
                    {
                        application.Resources[kvp.Key] = kvp.Value;
                    }

                    _logger.LogInformation("Applied {Theme} theme with {ResourceCount} resources", theme, themeDictionary.Count);
                }
                else
                {
                    _logger.LogError("Failed to find theme dictionary for {Theme}", theme);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying theme {Theme}", theme);
            }
        }

        private bool IsThemeResource(object key)
        {
            var keyString = key?.ToString();
            if (string.IsNullOrEmpty(keyString))
                return false;

            // Check if it's one of our theme resource keys
            return keyString.Contains("Primary") ||
                   keyString.Contains("Secondary") ||
                   keyString.Contains("Accent") ||
                   keyString.Contains("Background") ||
                   keyString.Contains("Text") ||
                   keyString.Contains("Gray") ||
                   keyString.Contains("Border") ||
                   keyString.Contains("Success") ||
                   keyString.Contains("Warning") ||
                   keyString.Contains("Error") ||
                   keyString.Contains("Info") ||
                   keyString.Contains("Hover") ||
                   keyString.Contains("Selected") ||
                   keyString.Contains("Active") ||
                   keyString.Contains("Focus") ||
                   keyString.Contains("Search") ||
                   keyString.Contains("File") ||
                   keyString.Contains("Sheet") ||
                   keyString.Contains("Highlight");
        }

        private Theme LoadThemePreference()
        {
            // TODO: Load from user settings or config file
            // For now, default to Light theme
            return Theme.Light;
        }

        private void SaveThemePreference(Theme theme)
        {
            // TODO: Save to user settings or config file
            _logger.LogDebug("Theme preference saved: {Theme}", theme);
        }
    }
}