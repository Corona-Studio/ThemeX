using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ThemeX.Helpers;

namespace ThemeX;

public class ThemeX : Styles
{
    public ThemeX(IServiceProvider? sp = null)
    {
        PopupOverlayHelper.EnsureInitialized();
        RevealHighlight.EnsureInitialized();
        AvaloniaXamlLoader.Load(sp, this);

        Settings = Resources.MergedDictionaries.OfType<ThemeSettings>().First();
    }

    public ThemeSettings Settings { get; }
}