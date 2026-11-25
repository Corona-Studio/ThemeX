using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using ThemeX.Helpers;

namespace ThemeX;

public partial class ThemeSettings
{
    public const string AccentColorKey = "ThemeX.AccentColor";

    public const string AccentColorLight1Key = "ThemeX.AccentColor.Lighter1";
    public const string AccentColorLight2Key = "ThemeX.AccentColor.Lighter2";
    public const string AccentColorLight3Key = "ThemeX.AccentColor.Lighter3";
    public const string AccentColorLight4Key = "ThemeX.AccentColor.Lighter4";
    public const string AccentColorLight5Key = "ThemeX.AccentColor.Lighter5";

    public const string AccentColorTransparentKey = "ThemeX.AccentColor.Transparent";
    public const string AccentColorTransparentColorKey = "ThemeX.AccentColor.Transparent.Color";

    public const string AccentColorLightTransparent1Key = "ThemeX.AccentColor.Transparent.Lighter1";
    public const string AccentColorLightTransparent2Key = "ThemeX.AccentColor.Transparent.Lighter2";
    public const string AccentColorLightTransparent3Key = "ThemeX.AccentColor.Transparent.Lighter3";
    public const string AccentColorLightTransparent4Key = "ThemeX.AccentColor.Transparent.Lighter4";

    public const string AccentColorDark1Key = "ThemeX.AccentColor.Darker1";
    public const string AccentColorDark2Key = "ThemeX.AccentColor.Darker2";
    public const string AccentColorDark3Key = "ThemeX.AccentColor.Darker3";
    public const string AccentColorDark4Key = "ThemeX.AccentColor.Darker4";
    public const string AccentColorDark5Key = "ThemeX.AccentColor.Darker5";

    public const string AccentColorDarkTransparent1Key = "ThemeX.AccentColor.Transparent.Darker1";
    public const string AccentColorDarkTransparent2Key = "ThemeX.AccentColor.Transparent.Darker2";
    public const string AccentColorDarkTransparent3Key = "ThemeX.AccentColor.Transparent.Darker3";
    public const string AccentColorDarkTransparent4Key = "ThemeX.AccentColor.Transparent.Darker4";

    public const string AccentColorForegroundKey = "ThemeX.AccentColor.Foreground";
    public const string AccentColorLight1ForegroundKey = "ThemeX.AccentColor.Lighter1.Foreground";
    public const string AccentColorDark1ForegroundKey = "ThemeX.AccentColor.Darker.Foreground";

    public const string AccentColorTextKey = "ThemeX.AccentColor.Text";
    public const string AccentColorTextHoverKey = "ThemeX.AccentColor.Text.Hover";
    public const string AccentColorTextPressedKey = "ThemeX.AccentColor.Text.Pressed";
    public const string AccentColorTextVisitedKey = "ThemeX.AccentColor.Text.Visited";

    public const string AccentColorHoverKey = "ThemeX.AccentColor.Hover";
    public const string AccentColorPressedKey = "ThemeX.AccentColor.Pressed";
    public const string AccentColorSubtleKey = "ThemeX.AccentColor.Subtle";
    public const string AccentColorSubtleHoverKey = "ThemeX.AccentColor.Subtle.Hover";
    public const string AccentElevationBorderKey = "ThemeX.Elevation.AccentBorder";

    public const string AcrylicTintKey = "ThemeX.Acrylic.TintColor";

    public static readonly Color Light = Color.Parse("#fafafa");
    public static readonly Color Dark = Color.Parse("#212121");

    public static readonly Color DarkBackdrop = Color.FromRgb(0x20, 0x20, 0x20);
    public static readonly Color LightBackdrop = Color.FromRgb(0xF3, 0xF3, 0xF3);

    private static readonly Color DarkCardBackdrop = Color.FromRgb(0x30, 0x30, 0x30);

    public static readonly ImmutableSolidColorBrush LightBrush = new(Light);
    public static readonly ImmutableSolidColorBrush DarkBrush = new(Dark);

    private ImmutableSolidColorBrush _darkAccentTextBrush = LightBrush;
    private ImmutableSolidColorBrush _darkAccentTextHoverBrush = LightBrush;
    private ImmutableSolidColorBrush _darkAccentTextPressedBrush = LightBrush;
    private ImmutableSolidColorBrush _darkAccentTextVisitedBrush = LightBrush;

    private ImmutableSolidColorBrush _lightAccentTextBrush = DarkBrush;
    private ImmutableSolidColorBrush _lightAccentTextHoverBrush = DarkBrush;
    private ImmutableSolidColorBrush _lightAccentTextPressedBrush = DarkBrush;
    private ImmutableSolidColorBrush _lightAccentTextVisitedBrush = DarkBrush;

    public Color AccentColor { get; private set; }

    public static Color GetBackdropColor(ThemeVariant? theme)
    {
        return theme == ThemeVariant.Dark ? DarkBackdrop : LightBackdrop;
    }

    public void SetAccentColor(Color value)
    {
        AccentColor = value;
        _resourceDic[AccentColorKey] = new ImmutableSolidColorBrush(value);

        var light1 = value.ToLighter();
        var light2 = light1.ToLighter();
        var light3 = light2.ToLighter();
        var light4 = light3.ToLighter();
        var light5 = light4.ToLighter();
        _resourceDic[AccentColorLight1Key] = new ImmutableSolidColorBrush(light1);
        _resourceDic[AccentColorLight2Key] = new ImmutableSolidColorBrush(light2);
        _resourceDic[AccentColorLight3Key] = new ImmutableSolidColorBrush(light3);
        _resourceDic[AccentColorLight4Key] = new ImmutableSolidColorBrush(light4);
        _resourceDic[AccentColorLight5Key] = new ImmutableSolidColorBrush(light5);

        var accentTransparent = value.ToMoreTransparent();
        _resourceDic[AccentColorTransparentKey] = new ImmutableSolidColorBrush(accentTransparent);
        _resourceDic[AccentColorTransparentColorKey] = accentTransparent;

        var accentLightTransparent1 = light1.ToMoreTransparent();
        var accentLightTransparent2 = light2.ToMoreTransparent();
        var accentLightTransparent3 = light3.ToMoreTransparent();
        var accentLightTransparent4 = light4.ToMoreTransparent();
        _resourceDic[AccentColorLightTransparent1Key] = new ImmutableSolidColorBrush(accentLightTransparent1);
        _resourceDic[AccentColorLightTransparent2Key] = new ImmutableSolidColorBrush(accentLightTransparent2);
        _resourceDic[AccentColorLightTransparent3Key] = new ImmutableSolidColorBrush(accentLightTransparent3);
        _resourceDic[AccentColorLightTransparent4Key] = new ImmutableSolidColorBrush(accentLightTransparent4);

        var darker1 = value.ToDarker();
        var darker2 = darker1.ToDarker();
        var darker3 = darker2.ToDarker();
        var darker4 = darker3.ToDarker();
        var darker5 = darker4.ToDarker();
        _resourceDic[AccentColorDark1Key] = new ImmutableSolidColorBrush(darker1);
        _resourceDic[AccentColorDark2Key] = new ImmutableSolidColorBrush(darker2);
        _resourceDic[AccentColorDark3Key] = new ImmutableSolidColorBrush(darker3);
        _resourceDic[AccentColorDark4Key] = new ImmutableSolidColorBrush(darker4);
        _resourceDic[AccentColorDark5Key] = new ImmutableSolidColorBrush(darker5);

        var accentDarkTransparent1 = darker1.ToMoreTransparent();
        var accentDarkTransparent2 = darker2.ToMoreTransparent();
        var accentDarkTransparent3 = darker3.ToMoreTransparent();
        var accentDarkTransparent4 = darker4.ToMoreTransparent();
        _resourceDic[AccentColorDarkTransparent1Key] = new ImmutableSolidColorBrush(accentDarkTransparent1);
        _resourceDic[AccentColorDarkTransparent2Key] = new ImmutableSolidColorBrush(accentDarkTransparent2);
        _resourceDic[AccentColorDarkTransparent3Key] = new ImmutableSolidColorBrush(accentDarkTransparent3);
        _resourceDic[AccentColorDarkTransparent4Key] = new ImmutableSolidColorBrush(accentDarkTransparent4);

        var hoverColor = Color.FromArgb(230, value.R, value.G, value.B);
        var pressedColor = Color.FromArgb(204, value.R, value.G, value.B);
        var subtleColor = Color.FromArgb(41, value.R, value.G, value.B);
        var subtleHoverColor = Color.FromArgb(64, value.R, value.G, value.B);
        _resourceDic[AccentColorHoverKey] = new ImmutableSolidColorBrush(hoverColor);
        _resourceDic[AccentColorPressedKey] = new ImmutableSolidColorBrush(pressedColor);
        _resourceDic[AccentColorSubtleKey] = new ImmutableSolidColorBrush(subtleColor);
        _resourceDic[AccentColorSubtleHoverKey] = new ImmutableSolidColorBrush(subtleHoverColor);

        _resourceDic[AccentElevationBorderKey] = new LinearGradientBrush
        {
            StartPoint = new Avalonia.RelativePoint(0.5, 0, Avalonia.RelativeUnit.Relative),
            EndPoint = new Avalonia.RelativePoint(0.5, 1, Avalonia.RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.FromArgb(38, 255, 255, 255), 0),
                new GradientStop(Color.FromArgb(26, 255, 255, 255), 0.88),
                new GradientStop(Color.FromArgb(92, 0, 0, 0), 1)
            ]
        }.ToImmutable();

        var foreground = value.GetGrayLevel() < 0.5 ? Light : Dark;
        var foregroundLight1 = light1.GetGrayLevel() < 0.5 ? Light : Dark;
        var foregroundDark1 = darker1.GetGrayLevel() < 0.5 ? Light : Dark;
        _resourceDic[AccentColorForegroundKey] = new ImmutableSolidColorBrush(foreground);
        _resourceDic[AccentColorLight1ForegroundKey] = new ImmutableSolidColorBrush(foregroundLight1);
        _resourceDic[AccentColorDark1ForegroundKey] = new ImmutableSolidColorBrush(foregroundDark1);

        UpdateThemeAwareAccentTextBrushes(value);

        RaiseResourcesChanged();
    }

    public void SetAcrylicTint(Color value)
    {
        _resourceDic[AcrylicTintKey] = value;

        RaiseResourcesChanged();
    }

    private bool TryGetThemeAwareAccentResource(string key, ThemeVariant? theme, out object? value)
    {
        var isDark = theme == ThemeVariant.Dark;

        value = key switch
        {
            AccentColorTextKey => isDark ? _darkAccentTextBrush : _lightAccentTextBrush,
            AccentColorTextHoverKey => isDark ? _darkAccentTextHoverBrush : _lightAccentTextHoverBrush,
            AccentColorTextPressedKey => isDark ? _darkAccentTextPressedBrush : _lightAccentTextPressedBrush,
            AccentColorTextVisitedKey => isDark ? _darkAccentTextVisitedBrush : _lightAccentTextVisitedBrush,
            _ => null
        };

        return value is not null;
    }

    private void UpdateThemeAwareAccentTextBrushes(Color accent)
    {
        var opaqueAccent = Color.FromRgb(accent.R, accent.G, accent.B);

        // Dark mode: guarantee WCAG AA (>= 5.2:1) on Dark cards (#303030) and >= 6.4:1 on Dark canvas (#202020).
        var darkText = EnsureContrastOnSurface(opaqueAccent, DarkCardBackdrop, 5.2, lighten: true);
        var darkHover = ShiftLightness(darkText, +0.08, 0.96);
        var darkPressed = EnsureContrastOnSurface(ShiftLightness(darkText, -0.06, 0.96), DarkCardBackdrop, 4.5, lighten: true);
        var darkVisited = ShiftLightness(darkText, +0.05, 0.96);

        _darkAccentTextBrush = new ImmutableSolidColorBrush(darkText);
        _darkAccentTextHoverBrush = new ImmutableSolidColorBrush(darkHover);
        _darkAccentTextPressedBrush = new ImmutableSolidColorBrush(darkPressed);
        _darkAccentTextVisitedBrush = new ImmutableSolidColorBrush(darkVisited);

        // Light mode: guarantee WCAG AA (>= 4.8:1) on Light canvas (#F3F3F3) and >= 5.2:1 on Light cards (#FDFDFD).
        var lightText = EnsureContrastOnSurface(opaqueAccent, LightBackdrop, 4.8, lighten: false);
        var lightHover = ShiftLightness(lightText, -0.07, 0.92);
        var lightPressed = ShiftLightness(lightText, -0.12, 0.92);
        var lightVisited = ShiftLightness(lightText, -0.09, 0.92);

        _lightAccentTextBrush = new ImmutableSolidColorBrush(lightText);
        _lightAccentTextHoverBrush = new ImmutableSolidColorBrush(lightHover);
        _lightAccentTextPressedBrush = new ImmutableSolidColorBrush(lightPressed);
        _lightAccentTextVisitedBrush = new ImmutableSolidColorBrush(lightVisited);
    }

    private static Color EnsureContrastOnSurface(Color color, Color surface, double targetContrast, bool lighten)
    {
        if (color.GetContrastRatio(surface) >= targetContrast)
            return color;

        var hsl = color.ToHsl();
        var lo = lighten ? hsl.L : 0.0;
        var hi = lighten ? 1.0 : hsl.L;
        var best = lighten ? Colors.White : Colors.Black;

        for (var i = 0; i < 24; i++)
        {
            var mid = (lo + hi) * 0.5;
            var candidate = new HslColor(1.0, hsl.H, hsl.S, mid).ToRgb();

            if (candidate.GetContrastRatio(surface) >= targetContrast)
            {
                best = candidate;
                if (lighten)
                    hi = mid;
                else
                    lo = mid;
            }
            else
            {
                if (lighten)
                    lo = mid;
                else
                    hi = mid;
            }
        }

        return best;
    }

    private static Color ShiftLightness(Color color, double delta, double maxLightness)
    {
        var hsl = color.ToHsl();
        var targetL = Math.Clamp(hsl.L + delta, 0.08, maxLightness);
        return new HslColor(1.0, hsl.H, hsl.S, targetL).ToRgb();
    }
}