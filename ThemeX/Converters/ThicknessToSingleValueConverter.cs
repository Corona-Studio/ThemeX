using System.Globalization;
using Avalonia;

namespace ThemeX.Converters;

public class ThicknessToSingleValueConverter : SingleWayConverter
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Thickness thickness) return 0;
        if (parameter is not string param) return 0;

        return param switch
        {
            "Left" => thickness.Left,
            "Top" => thickness.Top,
            "Right" => thickness.Right,
            "Bottom" => thickness.Bottom,
            _ => 0
        };
    }
}