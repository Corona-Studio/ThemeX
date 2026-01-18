using System.Globalization;
using Avalonia;

namespace ThemeX.Converters;

public class FitSquarelyWithinAspectRatioConverter : SingleWayConverter
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Rect bounds)
            return 0;

        return Math.Min(bounds.Width, bounds.Height);
    }
}