namespace ThemeX;

public partial class ThemeSettings
{
    public const string FontSizeWindowTitleKey = "ThemeX.FontSize.MainWindow";
    public const string FontSizeNormalTextKey  = "ThemeX.FontSize.Normal.Primary";
    public const string FontSizeNormalTextSecondaryKey  = "ThemeX.FontSize.Normal.Secondary";
    public const string FontSizeTabItemKey   = "ThemeX.FontSize.TabItem.Primary";
    public const string FontSizeTitleKey = "ThemeX.FontSize.Title.Primary";
    public const string FontSizeTitleSecondaryKey = "ThemeX.FontSize.Title.Secondary";
    
    public void SetFontSizeWindowTitle(double value)
    {
        _resourceDic[FontSizeWindowTitleKey] = value;
        RaiseResourcesChanged();
    }

    public void SetFontSizeNormalText(double value)
    {
        _resourceDic[FontSizeNormalTextKey]  = value;
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