using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using ThemeX.Helpers;

namespace ThemeX.Controls;

public sealed partial class Badge : ContentControl
{
    public static readonly StyledProperty<string> Symbol1Property =
        AvaloniaProperty.Register<Badge, string>(nameof(Symbol1));

    public static readonly StyledProperty<string> Symbol2Property =
        AvaloniaProperty.Register<Badge, string>(nameof(Symbol2));

    [GeneratedStyledProperty]
    public partial string Symbol { get; set; }

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

        ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        OnBackgroundChanged();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ActualThemeVariantChanged -= OnActualThemeVariantChanged;

        base.OnDetachedFromVisualTree(e);
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        OnBackgroundChanged();
    }

    private void OnBackgroundChanged()
    {
        if (!TryReadBackgroundColor(Background, out var color))
        {
            ClearValue(ForegroundProperty);
            return;
        }

        var backdrop = ResolveBackdropColor();
        var foreground = color.GetGrayLevel(backdrop) < 0.5 ? ThemeSettings.LightBrush : ThemeSettings.DarkBrush;

        SetValue(ForegroundProperty, foreground);
    }

    private Color ResolveBackdropColor()
    {
        var variant = ActualThemeVariant;
        if (variant != ThemeVariant.Dark && variant != ThemeVariant.Light)
            variant = Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;

        return ThemeSettings.GetBackdropColor(variant);
    }

    private static bool TryReadBackgroundColor(IBrush? brush, out Color color)
    {
        switch (brush)
        {
            case null:
                color = default;
                return false;

            // Immutable brushes are not AvaloniaObjects and are safe to read from any thread.
            case IImmutableSolidColorBrush immutable:
                color = ApplyBrushOpacity(immutable.Color, immutable.Opacity);
                return true;

            // Mutable SolidColorBrush is an AvaloniaObject affined to its owning dispatcher.
            // Reading Color off-thread throws InvalidOperationException (VerifyAccess).
            case SolidColorBrush solid:
                if (Dispatcher.UIThread.CheckAccess())
                    try
                    {
                        color = ApplyBrushOpacity(solid.Color, solid.Opacity);
                        return true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Brush is owned by a different dispatcher; bail out gracefully.
                        color = default;
                        return false;
                    }

                color = default;
                return false;

            default:
                color = default;
                return false;
        }
    }

    private static Color ApplyBrushOpacity(Color color, double opacity)
    {
        if (opacity >= 1.0)
            return color;

        var effectiveAlpha = (byte)Math.Clamp(Math.Round(color.A * Math.Clamp(opacity, 0.0, 1.0)), 0, 255);
        return Color.FromArgb(effectiveAlpha, color.R, color.G, color.B);
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