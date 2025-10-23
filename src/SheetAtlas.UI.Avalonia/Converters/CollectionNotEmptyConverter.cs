using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SheetAtlas.UI.Avalonia.Converters
{
    /// <summary>
    /// Converts a collection to a boolean indicating if it's not empty
    /// </summary>
    public class CollectionNotEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
