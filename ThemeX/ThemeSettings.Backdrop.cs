using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace ThemeX;

public partial class ThemeSettings
{
    public const string EnableBackdropEffectKey = "ThemeX.Backdrop.IsEnabled";
    public const string AcrylicBlurEffectKey = "ThemeX.Backdrop.AcrylicEffect";
    public const string FlyoutBlurEffectKey = "ThemeX.Backdrop.FlyoutEffect";
    public const string CardBlurEffectKey = "ThemeX.Backdrop.CardEffect";
    public const string SubtleBlurEffectKey = "ThemeX.Backdrop.SubtleEffect";

    public const string EffectiveAcrylicSurfaceKey = "ThemeX.Acrylic.EffectiveSurface";
    public const string EffectiveAcrylicFlyoutKey = "ThemeX.Acrylic.EffectiveFlyout";
    public const string EffectiveAcrylicCardKey = "ThemeX.Acrylic.EffectiveCard";
    public const string EffectiveAcrylicSubtleKey = "ThemeX.Acrylic.EffectiveSubtle";

    private static readonly IEffect DefaultAcrylicBlur = new BlurEffect { Radius = 32 }.ToImmutable();
    private static readonly IEffect DefaultFlyoutBlur = new BlurEffect { Radius = 28 }.ToImmutable();
    private static readonly IEffect DefaultCardBlur = new BlurEffect { Radius = 20 }.ToImmutable();
    private static readonly IEffect DefaultSubtleBlur = new BlurEffect { Radius = 16 }.ToImmutable();

    private static readonly ImmutableSolidColorBrush LightAcrylicSurface = new(Color.Parse("#C4F9F9F9"));
    private static readonly ImmutableSolidColorBrush LightAcrylicSurfaceFallback = new(Color.Parse("#FFF9F9F9"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicSurface = new(Color.Parse("#C4242424"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicSurfaceFallback = new(Color.Parse("#FF242424"));

    private static readonly ImmutableSolidColorBrush LightAcrylicFlyout = new(Color.Parse("#D8FCFCFC"));
    private static readonly ImmutableSolidColorBrush LightAcrylicFlyoutFallback = new(Color.Parse("#FFFCFCFC"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicFlyout = new(Color.Parse("#D82B2B2B"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicFlyoutFallback = new(Color.Parse("#FF2C2C2C"));

    private static readonly ImmutableSolidColorBrush LightAcrylicCard = new(Color.Parse("#B8FFFFFF"));
    private static readonly ImmutableSolidColorBrush LightAcrylicCardFallback = new(Color.Parse("#FFFFFFFF"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicCard = new(Color.Parse("#B02B2B2B"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicCardFallback = new(Color.Parse("#FF2B2B2B"));

    private static readonly ImmutableSolidColorBrush LightAcrylicSubtle = new(Color.Parse("#9CF5F5F5"));
    private static readonly ImmutableSolidColorBrush LightAcrylicSubtleFallback = new(Color.Parse("#FFF5F5F5"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicSubtle = new(Color.Parse("#99202020"));
    private static readonly ImmutableSolidColorBrush DarkAcrylicSubtleFallback = new(Color.Parse("#FF202020"));

    public bool IsBackdropEffectEnabled { get; private set; }

    public void SetEnableBackdropEffect(bool value)
    {
        IsBackdropEffectEnabled = value;
        _resourceDic[EnableBackdropEffectKey] = value;
        _resourceDic[AcrylicBlurEffectKey] = value ? DefaultAcrylicBlur : null;
        _resourceDic[FlyoutBlurEffectKey] = value ? DefaultFlyoutBlur : null;
        _resourceDic[CardBlurEffectKey] = value ? DefaultCardBlur : null;
        _resourceDic[SubtleBlurEffectKey] = value ? DefaultSubtleBlur : null;

        RaiseResourcesChanged();
    }

    private bool TryGetThemeAwareBackdropResource(string key, ThemeVariant? theme, out object? value)
    {
        var isDark = theme == ThemeVariant.Dark;
        var enabled = IsBackdropEffectEnabled;

        value = key switch
        {
            EffectiveAcrylicSurfaceKey => isDark
                ? enabled ? DarkAcrylicSurface : DarkAcrylicSurfaceFallback
                : enabled ? LightAcrylicSurface : LightAcrylicSurfaceFallback,
            EffectiveAcrylicFlyoutKey => isDark
                ? enabled ? DarkAcrylicFlyout : DarkAcrylicFlyoutFallback
                : enabled ? LightAcrylicFlyout : LightAcrylicFlyoutFallback,
            EffectiveAcrylicCardKey => isDark
                ? enabled ? DarkAcrylicCard : DarkAcrylicCardFallback
                : enabled ? LightAcrylicCard : LightAcrylicCardFallback,
            EffectiveAcrylicSubtleKey => isDark
                ? enabled ? DarkAcrylicSubtle : DarkAcrylicSubtleFallback
                : enabled ? LightAcrylicSubtle : LightAcrylicSubtleFallback,
            _ => null
        };

        return value is not null;
    }
}
