using System.Globalization;
using Avalonia.Media;
using ThemeX.Helpers;

namespace ThemeX.Converters;

public sealed class GreyLevelToForegroundConverter : SingleWayConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IBrush colorBrush)
            return null;

        return Convert(colorBrush);
    }

    public static IBrush Convert(IBrush? inBrush)
    {
        if (inBrush is not ISolidColorBrush colorBrush) return new SolidColorBrush(Colors.White);

        var greyLevel = colorBrush.Color.GetGrayLevel();

        return greyLevel < 0.5 ? ThemeSettings.LightBrush : ThemeSettings.DarkBrush;
    }
}