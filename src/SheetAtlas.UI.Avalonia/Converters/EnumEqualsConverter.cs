using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SheetAtlas.UI.Avalonia.Converters;

public class EnumEqualsConverter : IValueConverter
{
    public static readonly EnumEqualsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        // Convert both to strings and compare
        var valueString = value.ToString();
        var parameterString = parameter.ToString();

        return string.Equals(valueString, parameterString, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
