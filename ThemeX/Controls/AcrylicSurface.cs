using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ThemeX.Helpers;

namespace ThemeX.Controls;

public enum AcrylicVariant
{
    Pane,
    Flyout,
    Card,
    Subtle
}

/// <summary>
///     Fluent Design 2 translucent surface powered by Avalonia 12.2 <see cref="Visual.BackdropEffect" />.
///     Separates outer elevation shadow from the rounded, clipped backdrop layer and falls back
///     cleanly to an opaque surface when backdrop effects are disabled.
/// </summary>
[TemplatePart("PART_BackdropBorder", typeof(Border))]
[PseudoClasses(":pane", ":flyout", ":card", ":subtle", ":backdrop-enabled", ":backdrop-fallback", ":volatile-cache", ":shadow")]
public partial class AcrylicSurface : ContentControl
{
    private bool _isInNativePopupRoot;
    private Border? _backdropBorder;
    private Size _lastBackdropClipSize;
    private CornerRadius _lastBackdropClipCornerRadius;
    private bool _hasBackdropClipState;

    public AcrylicSurface()
    {
        UpdatePseudoClasses();
    }

    [GeneratedStyledProperty(DefaultValue = AcrylicVariant.Pane)]
    public partial AcrylicVariant Variant { get; set; }

    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool IsBackdropEnabled { get; set; }

    /// <summary>
    ///     When null, <see cref="AcrylicVariant.Flyout" /> and <see cref="AcrylicVariant.Subtle" /> use Volatile
    ///     caching while <see cref="AcrylicVariant.Pane" /> and <see cref="AcrylicVariant.Card" /> use Retained caching.
    ///     Set to <c>true</c> whenever this surface is hosted under an ancestor <see cref="Visual.Effect" />,
    ///     <see cref="Visual.OpacityMask" />, or animated popup transition.
    /// </summary>
    [GeneratedStyledProperty]
    public partial bool? UseVolatileCache { get; set; }

    /// <summary>
    ///     When null, <see cref="AcrylicVariant.Flyout" /> shows its elevation shadow automatically.
    /// </summary>
    [GeneratedStyledProperty]
    public partial bool? ShowElevationShadow { get; set; }

    [GeneratedStyledProperty]
    public partial IEffect? BlurEffect { get; set; }

    [GeneratedStyledProperty]
    public partial IBrush? TintBrush { get; set; }

    [GeneratedStyledProperty]
    public partial IBrush? FallbackBrush { get; set; }

    [GeneratedStyledProperty(DefaultValue = 1d)]
    public partial double TintOpacity { get; set; }

    [GeneratedStyledProperty]
    public partial BoxShadows ElevationShadow { get; set; }

    [GeneratedStyledProperty]
    public partial IBrush? SpecularBorderBrush { get; set; }

    [GeneratedStyledProperty]
    public partial IBrush? LuminosityBrush { get; set; }

    [GeneratedStyledProperty]
    public partial IBrush? SheenBrush { get; set; }

    [GeneratedStyledProperty(DefaultValue = false)]
    public partial bool IsRevealEnabled { get; set; }

    [GeneratedStyledProperty(DefaultValue = RevealHighlightMode.BorderAndHover)]
    public partial RevealHighlightMode RevealMode { get; set; }

    [GeneratedStyledProperty(DefaultValue = RevealHighlightTone.Standard)]
    public partial RevealHighlightTone RevealTone { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_backdropBorder is not null)
        {
            _backdropBorder.PropertyChanged -= OnBackdropBorderPropertyChanged;
            _backdropBorder.Clip = null;
        }

        _backdropBorder = e.NameScope.Find<Border>("PART_BackdropBorder");
        _hasBackdropClipState = false;

        if (_backdropBorder is not null)
        {
            _backdropBorder.PropertyChanged += OnBackdropBorderPropertyChanged;
            UpdateBackdropClip(force: true);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Visual.BackdropEffect samples only pixels inside the same TopLevel compositor surface.
        // A standalone native PopupRoot has nothing behind it, so fall back to an opaque Fluent surface.
        _isInNativePopupRoot = TopLevel.GetTopLevel(this) is PopupRoot;
        UpdatePseudoClasses();
        UpdateBackdropClip();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isInNativePopupRoot = false;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == VariantProperty ||
            change.Property == IsBackdropEnabledProperty ||
            change.Property == UseVolatileCacheProperty ||
            change.Property == ShowElevationShadowProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == CornerRadiusProperty)
        {
            UpdateBackdropClip();
        }
    }

    private void OnBackdropBorderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        // Clip geometry uses local size, independent of the border's position.
        // Skip position-only Bounds updates (e.g. scrolling or container moves).
        if (change.Property == BoundsProperty)
        {
            if (change.GetOldValue<Rect>().Size != change.GetNewValue<Rect>().Size)
                UpdateBackdropClip();
        }
        else if (change.Property == Border.CornerRadiusProperty)
        {
            UpdateBackdropClip();
        }
    }

    private void UpdateBackdropClip(bool force = false)
    {
        if (_backdropBorder is null)
            return;

        var size = _backdropBorder.Bounds.Size;
        var cornerRadius = _backdropBorder.CornerRadius;

        if (!force &&
            _hasBackdropClipState &&
            _lastBackdropClipSize == size &&
            _lastBackdropClipCornerRadius == cornerRadius)
        {
            return;
        }

        _lastBackdropClipSize = size;
        _lastBackdropClipCornerRadius = cornerRadius;
        _hasBackdropClipState = true;

        var clip = GeometryHelper.CreateRoundedClipGeometry(size, cornerRadius);
        if (clip is null && _backdropBorder.Clip is null)
            return;

        _backdropBorder.Clip = clip;
    }

    private void UpdatePseudoClasses()
    {
        var variant = Variant;
        PseudoClasses.Set(":pane", variant == AcrylicVariant.Pane);
        PseudoClasses.Set(":flyout", variant == AcrylicVariant.Flyout);
        PseudoClasses.Set(":card", variant == AcrylicVariant.Card);
        PseudoClasses.Set(":subtle", variant == AcrylicVariant.Subtle);

        var backdropEnabled = IsBackdropEnabled && !_isInNativePopupRoot;
        PseudoClasses.Set(":backdrop-enabled", backdropEnabled);
        PseudoClasses.Set(":backdrop-fallback", !backdropEnabled);

        var useVolatile = UseVolatileCache ?? variant is AcrylicVariant.Flyout or AcrylicVariant.Subtle;
        PseudoClasses.Set(":volatile-cache", useVolatile);

        var showShadow = ShowElevationShadow ?? variant == AcrylicVariant.Flyout;
        PseudoClasses.Set(":shadow", showShadow);
    }
}
