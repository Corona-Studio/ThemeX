using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ThemeX.Helpers;

namespace ThemeX.Controls;

public sealed class Badge : ContentControl
{
    public static readonly StyledProperty<string> SymbolProperty =
        AvaloniaProperty.Register<Badge, string>(nameof(Symbol));

    public static readonly StyledProperty<string> Symbol1Property =
        AvaloniaProperty.Register<Badge, string>(nameof(Symbol1));

    public static readonly StyledProperty<string> Symbol2Property =
        AvaloniaProperty.Register<Badge, string>(nameof(Symbol2));

    public string Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    private string Symbol1
    {
        get => GetValue(Symbol1Property);
        set => SetValue(Symbol1Property, value);
    }

    private string Symbol2
    {
        get => GetValue(Symbol2Property);
        set => SetValue(Symbol2Property, value);
    }

    protected override Type StyleKeyOverride => typeof(Badge);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        OnBackgroundChanged();
    }

    private void OnBackgroundChanged()
    {
        var primaryColor = (ISolidColorBrush?)Background;
        var foreground = primaryColor?.Color.GetGrayLevel() < 0.5 ? ThemeSettings.LightBrush : ThemeSettings.DarkBrush;

        SetValue(ForegroundProperty, foreground);
    }

    private void OnSymbolChanged()
    {
        if (!string.IsNullOrEmpty(Symbol))
        {
            var symbols = Symbol.Split(',');

            Symbol1 = symbols[0].Trim();

            if (symbols.Length == 2)
                Symbol2 = symbols[1].Trim();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SymbolProperty)
            OnSymbolChanged();
        if (change.Property == BackgroundProperty)
            OnBackgroundChanged();

        base.OnPropertyChanged(change);
    }
}