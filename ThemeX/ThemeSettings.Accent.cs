using Avalonia.Media;
using Avalonia.Media.Immutable;
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

    public const string AcrylicTintKey = "ThemeX.Acrylic.TintColor";

    public static readonly Color Light = Color.Parse("#fafafa");
    public static readonly Color Dark = Color.Parse("#000000");

    public static readonly ImmutableSolidColorBrush LightBrush = new(Light);
    public static readonly ImmutableSolidColorBrush DarkBrush = new(Dark);

    public void SetAccentColor(Color value)
    {
        _resourceDic[AccentColorKey] = new ImmutableSolidColorBrush(value);

        var light1 = value.ToLighter();
        var light2 = light1.ToLighter();
        var light3 = light2.ToLighter();
        var light4 = light3.ToLighter();
        var light5 = light3.ToLighter();
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

        var foreground = value.GetGrayLevel() < 0.5 ? Light : Dark;
        var foregroundLight1 = light1.GetGrayLevel() < 0.5 ? Light : Dark;
        var foregroundDark1 = darker1.GetGrayLevel() < 0.5 ? Light : Dark;
        _resourceDic[AccentColorForegroundKey] = new ImmutableSolidColorBrush(foreground);
        _resourceDic[AccentColorLight1ForegroundKey] = new ImmutableSolidColorBrush(foregroundLight1);
        _resourceDic[AccentColorDark1ForegroundKey] = new ImmutableSolidColorBrush(foregroundDark1);

        RaiseResourcesChanged();
    }

    public void SetAcrylicTint(Color value)
    {
        _resourceDic[AcrylicTintKey] = value;

        RaiseResourcesChanged();
    }
}