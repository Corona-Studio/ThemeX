using System.Globalization;
using Avalonia;
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
        var backdrop = ThemeSettings.GetBackdropColor(Application.Current?.ActualThemeVariant);
        return Convert(inBrush, backdrop);
    }

    public static IBrush Convert(IBrush? inBrush, Color backdrop)
    {
        if (inBrush is not ISolidColorBrush colorBrush)
            return ThemeSettings.LightBrush;

        var color = colorBrush.Color;
        if (colorBrush.Opacity < 1.0)
        {
            var effectiveAlpha = (byte)Math.Clamp(
                Math.Round(color.A * Math.Clamp(colorBrush.Opacity, 0.0, 1.0)),
                0,
                255);
            color = Color.FromArgb(effectiveAlpha, color.R, color.G, color.B);
        }

        var greyLevel = color.GetGrayLevel(backdrop);

        return greyLevel < 0.5 ? ThemeSettings.LightBrush : ThemeSettings.DarkBrush;
    }
}