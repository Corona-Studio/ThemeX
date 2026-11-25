using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace ThemeX;

public sealed partial class ThemeSettings : ResourceProvider
{
    private readonly Dictionary<string, object?> _resourceDic = [];

    public ThemeSettings()
    {
        SetAccentColor(Color.Parse("#FFB900"));
        SetEnableFluentScroll(true);
        SetEnableBackdropEffect(true);
        SetAcrylicTint(Colors.White);
        SetAnimationDuration(TimeSpan.FromMilliseconds(450));
        SetAnimationEasing(new SplineEasing(0.1, 0.9, 0.2));
        SetEnableAnimation(true);
        SetEnableRevealEffect(true);

        SetFontSizeWindowTitle(17);
        SetFontSizeNormalText(14);
        SetFontSizeNormalTextSecondary(12);
        SetFontSizeTabItem(16);
        SetFontSizeTitle(18);
        SetFontSizeTitleSecondary(14);
        SetFontSizeIconPrimary(14);
        SetIconFontSizeBig1Text(24);

        SetFontSizeCaption(12);
        SetFontSizeBody(14);
        SetFontSizeBodyLarge(16);
        SetFontSizeSubtitle(20);
        SetFontSizeFluentTitle(24);
        SetFontSizeTitleLarge(28);
        SetFontSizeDisplay(36);
    }

    public override bool HasResources => true;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (key is string strKey)
        {
            if (TryGetThemeAwareBackdropResource(strKey, theme, out value))
                return true;

            if (TryGetThemeAwareAccentResource(strKey, theme, out value))
                return true;

            return _resourceDic.TryGetValue(strKey, out value);
        }

        value = null;
        return false;
    }
}