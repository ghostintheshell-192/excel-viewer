using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ExcelViewer.UI.Avalonia.Models;

namespace ExcelViewer.UI.Avalonia.Converters
{
    /// <summary>
    /// Converts ComparisonType to appropriate background brush for visual distinction
    /// </summary>
    public class ComparisonTypeToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Debug: Let's see what the converter is actually receiving
            System.Diagnostics.Debug.WriteLine($"Converter received: {value} (type: {value?.GetType()})");

            if (value is not ComparisonType comparisonType)
            {
                System.Diagnostics.Debug.WriteLine("Value is NOT ComparisonType - using default");
                return GetResource("ComparisonMatchBackground");
            }

            System.Diagnostics.Debug.WriteLine($"Converter processing: {comparisonType}");

            var resourceKey = comparisonType switch
            {
                ComparisonType.Match => "ComparisonMatchBackground",
                ComparisonType.Different => "ComparisonDifferentBackground",
                ComparisonType.New => "ComparisonNewBackground",
                ComparisonType.Missing => "ComparisonMissingBackground",
                _ => "ComparisonMatchBackground"
            };

            System.Diagnostics.Debug.WriteLine($"Using resource key: {resourceKey}");
            var result = GetResource(resourceKey);
            System.Diagnostics.Debug.WriteLine($"GetResource returned: {result}");

            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static object GetResource(string key)
        {
            if (Application.Current?.Resources.TryGetResource(key, null, out var resource) == true)
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