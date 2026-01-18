using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace ThemeX;

public class ThemeX : Styles
{
    public ThemeX(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);

        Settings = Resources.MergedDictionaries.OfType<ThemeSettings>().First();
    }

    public ThemeSettings Settings { get; }
}