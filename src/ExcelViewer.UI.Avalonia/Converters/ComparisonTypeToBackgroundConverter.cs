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
            if (value is not ComparisonType comparisonType)
                return GetResource("ComparisonMatchBackground");

            return comparisonType switch
            {
                ComparisonType.Match => GetResource("ComparisonMatchBackground"),
                ComparisonType.Different => GetResource("ComparisonDifferentBackground"),
                ComparisonType.New => GetResource("ComparisonNewBackground"),
                ComparisonType.Missing => GetResource("ComparisonMissingBackground"),
                _ => GetResource("ComparisonMatchBackground")
            };
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