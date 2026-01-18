using Avalonia.Data.Converters;
using CommunityToolkit.Diagnostics;
using System.Globalization;

namespace ThemeX.Converters;

public abstract class SingleWayConverter : IValueConverter
{
    public abstract object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ThrowHelper.ThrowInvalidOperationException<object>();
    }
}