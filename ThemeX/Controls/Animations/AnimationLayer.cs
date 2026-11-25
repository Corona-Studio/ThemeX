using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using ThemeX.Helpers;

namespace ThemeX.Controls.Animations;

[TemplatePart(Name = "ActiveRect", IsRequired = true)]
[TemplatePart(Name = "PART_RippleCanvasRoot", IsRequired = true)]
public partial class AnimationLayer : ContentControl
{
    public static readonly StyledProperty<ISolidColorBrush> MouseOnBrushProperty =
        AvaloniaProperty.Register<AnimationLayer, ISolidColorBrush>(
            nameof(MouseOnBrush),
            new ImmutableSolidColorBrush(Color.FromArgb(90, 0, 0, 0)));

    private bool _isPointerOver;

    protected Rectangle? ActiveRect;
    protected Canvas? PART_RippleCanvasRoot;

    [Category("Animation")]
    public ISolidColorBrush MouseOnBrush
    {
        get => GetValue(MouseOnBrushProperty);
        set => SetValue(MouseOnBrushProperty, value);
    }

    /// <summary>
    ///     鼠标移入移出动画
    /// </summary>
    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool MouseEnterAnimation { get; set; }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (!_isPointerOver)
        {
            _isPointerOver = true;
            BeginMouseEnterAnim(e);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (!IsPointerOver)
        {
            _isPointerOver = false;
            BeginMouseLeaveAnim(e);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Find canvas host
        ActiveRect = e.NameScope.Find<Rectangle>(nameof(ActiveRect));
        PART_RippleCanvasRoot = e.NameScope.Find<Canvas>(nameof(PART_RippleCanvasRoot));
    }

    protected virtual void BeginMouseEnterAnim(PointerEventArgs? e)
    {
        if (ActiveRect != null && MouseEnterAnimation)
            ActiveRect.Opacity = 1;
    }

    protected virtual void BeginMouseLeaveAnim(PointerEventArgs? e)
    {
        if (ActiveRect != null && MouseEnterAnimation)
            ActiveRect.Opacity = 0;
    }

    private void OnMouseEnterAnimationChanged()
    {
        if (Tag == null) return;

        if (!MouseEnterAnimation)
            BeginMouseEnterAnim(null);
        else
            BeginMouseLeaveAnim(null);
    }

    private void OnCornerRadiusChanged()
    {
        var radius = CornerRadius.GetAverageCornerRadius();
        var clip = GeometryHelper.CreateDefiningGeometry(Bounds.Size, radiusX: radius, radiusY: radius);

        Clip = clip;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == MouseEnterAnimationProperty)
            OnMouseEnterAnimationChanged();
        // Clip geometry uses local size, independent of the layer's position.
        // Scrolling and rearranging rows must not rebuild the same native path.
        if (change.Property == CornerRadiusProperty ||
            change.Property == BoundsProperty && change.GetOldValue<Rect>().Size != change.GetNewValue<Rect>().Size)
            OnCornerRadiusChanged();

        base.OnPropertyChanged(change);
    }
}
