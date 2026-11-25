namespace ThemeX;

public partial class ThemeSettings
{
    public const string FontSizeWindowTitleKey = "ThemeX.FontSize.MainWindow";
    public const string FontSizeNormalTextKey = "ThemeX.FontSize.Normal.Primary";
    public const string FontSizeNormalTextSecondaryKey = "ThemeX.FontSize.Normal.Secondary";
    public const string FontSizeTabItemKey = "ThemeX.FontSize.TabItem.Primary";
    public const string FontSizeTitleKey = "ThemeX.FontSize.Title.Primary";
    public const string FontSizeTitleSecondaryKey = "ThemeX.FontSize.Title.Secondary";
    public const string FontSizeBigIconKey = "ThemeX.FontSize.Icon.Big.1";
    public const string FontSizeIconKey = "ThemeX.FontSize.Icon.Primary";

    // Fluent 2 typography scale
    public const string FontSizeCaptionKey = "ThemeX.FontSize.Caption";
    public const string FontSizeBodyKey = "ThemeX.FontSize.Body";
    public const string FontSizeBodyLargeKey = "ThemeX.FontSize.BodyLarge";
    public const string FontSizeSubtitleKey = "ThemeX.FontSize.Subtitle";
    public const string FontSizeFluentTitleKey = "ThemeX.FontSize.Title";
    public const string FontSizeTitleLargeKey = "ThemeX.FontSize.TitleLarge";
    public const string FontSizeDisplayKey = "ThemeX.FontSize.Display";

    public void SetFontSizeCaption(double value)
    {
        _resourceDic[FontSizeCaptionKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeBody(double value)
    {
        _resourceDic[FontSizeBodyKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeBodyLarge(double value)
    {
        _resourceDic[FontSizeBodyLargeKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeSubtitle(double value)
    {
        _resourceDic[FontSizeSubtitleKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeFluentTitle(double value)
    {
        _resourceDic[FontSizeFluentTitleKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeTitleLarge(double value)
    {
        _resourceDic[FontSizeTitleLargeKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeDisplay(double value)
    {
        _resourceDic[FontSizeDisplayKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeIconPrimary(double value)
    {
        _resourceDic[FontSizeIconKey] = value;
        RaiseResourcesChanged();
    }

    public void SetIconFontSizeBig1Text(double value)
    {
        _resourceDic[FontSizeBigIconKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeWindowTitle(double value)
    {
        _resourceDic[FontSizeWindowTitleKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeNormalText(double value)
    {
        _resourceDic[FontSizeNormalTextKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeNormalTextSecondary(double value)
    {
        _resourceDic[FontSizeNormalTextSecondaryKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeTabItem(double value)
    {
        _resourceDic[FontSizeTabItemKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeTitle(double value)
    {
        _resourceDic[FontSizeTitleKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeTitleSecondary(double value)
    {
        _resourceDic[FontSizeTitleSecondaryKey] = value;
        RaiseResourcesChanged();
    }
}