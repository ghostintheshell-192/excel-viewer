using Avalonia.Controls;
using Avalonia.Data.Converters;
using System.Globalization;

namespace SheetAtlas.UI.Avalonia.Converters;

public class BoolToSidebarWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return new GridLength(isExpanded ? 300 : 0, GridUnitType.Pixel);
        }
        return new GridLength(0, GridUnitType.Pixel); // Default collapsed (hidden)
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
