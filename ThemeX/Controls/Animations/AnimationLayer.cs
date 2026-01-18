using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using ThemeX.Helpers;

namespace ThemeX.Controls.Animations;

public class AnimationLayer : ContentControl
{
    [Category("Animation")] public static readonly StyledProperty<Brush> MouseOnBrushProperty =
        AvaloniaProperty.Register<AnimationLayer, Brush>(
            nameof(MouseOnBrush), new SolidColorBrush(Color.FromArgb(90, 0, 0, 0)));

    public static readonly StyledProperty<bool> MouseEnterAnimationProperty =
        AvaloniaProperty.Register<AnimationLayer, bool>(
            nameof(MouseEnterAnimation),
            true
        );

    protected Rectangle? ActiveRect;
    protected Canvas? PART_RippleCanvasRoot;

    public Brush MouseOnBrush
    {
        get => GetValue(MouseOnBrushProperty);
        set => SetValue(MouseOnBrushProperty, value);
    }

    /// <summary>
    ///     鼠标移入移出动画
    /// </summary>
    public bool MouseEnterAnimation
    {
        get => GetValue(MouseEnterAnimationProperty);
        set => SetValue(MouseEnterAnimationProperty, value);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        BeginMouseEnterAnim(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        BeginMouseLeaveAnim(e);
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
        if (change.Property == CornerRadiusProperty || change.Property == BoundsProperty)
            OnCornerRadiusChanged();

        base.OnPropertyChanged(change);
    }
}