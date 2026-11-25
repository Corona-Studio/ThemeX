using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace ThemeX.Controls.IconPacks;

/// <summary>
///     Draws one or two icon-font glyphs in a single visual.
/// </summary>
public sealed partial class IconPack : TemplatedControl
{
    private LayoutCache _symbolLayout;
    private LayoutCache _overlayLayout;

    static IconPack()
    {
        AffectsMeasure<IconPack>(
            SymbolProperty,
            OverlaySymbolProperty,
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty,
            FontStretchProperty,
            PaddingProperty);

        AffectsRender<IconPack>(
            SymbolProperty,
            OverlaySymbolProperty,
            OverlayForegroundProperty,
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty,
            FontStretchProperty,
            ForegroundProperty,
            BackgroundProperty,
            PaddingProperty);
    }

    /// <summary>
    ///     Gets or sets the primary glyph.
    /// </summary>
    [GeneratedStyledProperty]
    public partial string? Symbol { get; set; }

    /// <summary>
    ///     Gets or sets an optional glyph drawn over <see cref="Symbol" />.
    /// </summary>
    [GeneratedStyledProperty]
    public partial string? OverlaySymbol { get; set; }

    /// <summary>
    ///     Gets or sets the overlay glyph brush. When unset, <see cref="TemplatedControl.Foreground" /> is used.
    /// </summary>
    [GeneratedStyledProperty]
    public partial IBrush? OverlayForeground { get; set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        var symbolLayout = GetSymbolLayout();
        var overlayLayout = GetOverlayLayout();
        var padding = Padding;

        return new Size(
            Math.Max(symbolLayout?.Width ?? 0, overlayLayout?.Width ?? 0) + padding.Left + padding.Right,
            Math.Max(symbolLayout?.Height ?? 0, overlayLayout?.Height ?? 0) + padding.Top + padding.Bottom);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Background is { } background)
            context.FillRectangle(background, new Rect(Bounds.Size));

        var padding = Padding;
        var contentBounds = new Rect(
            padding.Left,
            padding.Top,
            Math.Max(0, Bounds.Width - padding.Left - padding.Right),
            Math.Max(0, Bounds.Height - padding.Top - padding.Bottom));

        DrawCentered(context, GetSymbolLayout(), contentBounds);
        DrawCentered(context, GetOverlayLayout(), contentBounds);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _symbolLayout.Dispose();
        _overlayLayout.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private TextLayout? GetSymbolLayout()
    {
        return _symbolLayout.GetOrCreate(Symbol, Foreground, this);
    }

    private TextLayout? GetOverlayLayout()
    {
        return _overlayLayout.GetOrCreate(OverlaySymbol, OverlayForeground ?? Foreground, this);
    }

    private static void DrawCentered(DrawingContext context, TextLayout? layout, Rect bounds)
    {
        if (layout == null)
            return;

        var origin = new Point(
            bounds.X + (bounds.Width - layout.Width) / 2,
            bounds.Y + (bounds.Height - layout.Height) / 2);

        layout.Draw(context, origin);
    }

    private struct LayoutCache
    {
        private TextLayout? _layout;
        private LayoutKey? _key;

        public TextLayout? GetOrCreate(string? symbol, IBrush? foreground, IconPack owner)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                Dispose();
                return null;
            }

            var key = new LayoutKey(
                symbol,
                owner.FontFamily,
                owner.FontSize,
                owner.FontStyle,
                owner.FontWeight,
                owner.FontStretch,
                foreground);

            if (_layout != null && _key == key)
                return _layout;

            Dispose();
            _key = key;
            _layout = new TextLayout(
                symbol,
                new Typeface(key.FontFamily, key.FontStyle, key.FontWeight, key.FontStretch),
                key.FontSize,
                key.Foreground);

            return _layout;
        }

        public void Dispose()
        {
            _layout?.Dispose();
            _layout = null;
            _key = null;
        }
    }

    private readonly record struct LayoutKey(
        string Symbol,
        FontFamily FontFamily,
        double FontSize,
        FontStyle FontStyle,
        FontWeight FontWeight,
        FontStretch FontStretch,
        IBrush? Foreground);
}
