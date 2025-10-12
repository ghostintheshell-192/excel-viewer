using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SheetAtlas.UI.Avalonia.Models;

namespace SheetAtlas.UI.Avalonia.Converters
{
    /// <summary>
    /// Converts ComparisonType or CellComparisonResult to appropriate background brush for visual distinction
    /// Supports gradient coloring based on frequency intensity for different values
    /// </summary>
    public class ComparisonTypeToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Handle CellComparisonResult with intensity-based gradients
            if (value is CellComparisonResult comparisonResult)
            {
                return GetBrushForComparisonResult(comparisonResult);
            }

            // Fallback to default background
            return GetResource("ComparisonMatchBackground");
        }

        private static object GetBrushForComparisonResult(CellComparisonResult result)
        {
            // All background colors are now theme-aware
            bool isDarkMode = IsDarkMode();

            return result.Type switch
            {
                ComparisonType.Match => GetMatchBackground(isDarkMode),
                ComparisonType.Different => CreateGradientBrush(result.Intensity),
                ComparisonType.New => GetNewBackground(isDarkMode),
                ComparisonType.Missing => GetMissingBackground(isDarkMode),
                _ => GetMatchBackground(isDarkMode)
            };
        }

        private static SolidColorBrush GetMatchBackground(bool isDarkMode)
        {
            // Match: neutral background (same as main background)
            return isDarkMode
                ? new SolidColorBrush(Color.Parse("#0D1117"))  // Dark theme
                : new SolidColorBrush(Color.Parse("#FFFFFF")); // Light theme
        }

        private static SolidColorBrush GetNewBackground(bool isDarkMode)
        {
            // New: subtle green tint
            return isDarkMode
                ? new SolidColorBrush(Color.Parse("#0F1A14"))  // Dark green tint
                : new SolidColorBrush(Color.Parse("#F8FDF9")); // Light green tint
        }

        private static SolidColorBrush GetMissingBackground(bool isDarkMode)
        {
            // Missing: subtle red tint
            return isDarkMode
                ? new SolidColorBrush(Color.Parse("#1A1214"))  // Dark red tint
                : new SolidColorBrush(Color.Parse("#FEF8F8")); // Light red tint
        }


        /// <summary>
        /// Creates a gradient brush from light pink to dark red based on intensity
        /// Theme-aware: adapts lightness range for dark mode
        /// </summary>
        private static SolidColorBrush CreateGradientBrush(double intensity)
        {
            // Clamp intensity to valid range
            intensity = Math.Clamp(intensity, 0.0, 1.0);

            // Detect if we're in dark mode by checking MainBackground lightness
            bool isDarkMode = IsDarkMode();

            // HSL color model: H=340° (pink/red), S=70%, L=variable based on intensity
            double lightness, saturation;

            if (isDarkMode)
            {
                // Dark mode: darker range to maintain text readability
                // intensity=0.0 → L=20% (very dark pink)
                // intensity=1.0 → L=50% (medium red)
                lightness = 20 + (intensity * 30); // 20% to 50%
                saturation = 60 + (intensity * 20); // 60% to 80%
            }
            else
            {
                // Light mode: lighter range (original behavior)
                // intensity=0.0 → L=90% (light pink)
                // intensity=1.0 → L=50% (dark red)
                lightness = 90 - (intensity * 40); // 90% to 50%
                saturation = 70 + (intensity * 20); // 70% to 90%
            }

            // Convert HSL to RGB
            var color = HslToRgb(340, saturation / 100.0, lightness / 100.0);

            return new SolidColorBrush(color);
        }

        /// <summary>
        /// Detects if the current theme is dark mode by checking MainBackground lightness
        /// </summary>
        private static bool IsDarkMode()
        {
            if (Application.Current?.Resources.TryGetResource("MainBackground", null, out var resource) == true
                && resource is SolidColorBrush brush)
            {
                var color = brush.Color;
                // Calculate perceived lightness (simple approximation)
                var lightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
                // If lightness < 0.5, we're in dark mode
                return lightness < 0.5;
            }

            // Default to light mode if we can't determine
            return false;
        }

        /// <summary>
        /// Converts HSL color values to RGB Color
        /// </summary>
        private static Color HslToRgb(double h, double s, double l)
        {
            h = h / 360.0; // Convert to 0-1 range

            double r, g, b;

            if (Math.Abs(s) < 0.001)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                var hue2rgb = new Func<double, double, double, double>((p, q, t) =>
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1.0 / 6) return p + (q - p) * 6 * t;
                    if (t < 1.0 / 2) return q;
                    if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
                    return p;
                });

                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = hue2rgb(p, q, h + 1.0 / 3);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1.0 / 3);
            }

            return Color.FromRgb(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255)
            );
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static object GetResource(string key)
        {
            if (Application.Current?.Resources.TryGetResource(key, null, out var resource) == true && resource != null)
            {
                return resource;
            }

            // Fallback colors if resource not found
            return key switch
            {
                "ComparisonMatchBackground" => new SolidColorBrush(Colors.White),
                "ComparisonDifferentBackground" => new SolidColorBrush(Color.Parse("#FFE5D3")),
                "ComparisonNewBackground" => new SolidColorBrush(Color.Parse("#D4EDDA")),
                "ComparisonMissingBackground" => new SolidColorBrush(Color.Parse("#F8D7DA")),
                _ => new SolidColorBrush(Colors.White)
            };
        }
    }
}
