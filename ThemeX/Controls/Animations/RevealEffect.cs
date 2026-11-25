using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using ThemeX.Controls.Effects;
using ThemeX.Helpers;

namespace ThemeX.Controls.Animations;

public sealed partial class RevealEffect : AnimationLayer
{
    private static readonly IBrush MaskBrush = new RadialGradientBrush
    {
        GradientStops =
        [
            new GradientStop(Colors.Black, 0),
            new GradientStop(Color.FromArgb(0, 0, 0, 0), 1)
        ]
    }.ToImmutable();

    private Ellipse? _last;
    private byte _pointers;

    public RevealEffect()
    {
        MouseEnterAnimation = false;
    }

    [GeneratedStyledProperty]
    public partial ISolidColorBrush HaloBrush { get; set; }

    protected override Type StyleKeyOverride => typeof(AnimationLayer);

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_last == null || PART_RippleCanvasRoot == null) return;

        var p = e.GetPosition(PART_RippleCanvasRoot);
        var width = _last.Bounds.Width > 0 ? _last.Bounds.Width : _last.Width;
        var height = _last.Bounds.Height > 0 ? _last.Bounds.Height : _last.Height;

        Canvas.SetTop(_last, p.Y - height / 2);
        Canvas.SetLeft(_last, p.X - width / 2);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_pointers != 0 || PART_RippleCanvasRoot == null || !ThemeSettings.IsAnimationEnabled || !ThemeSettings.IsRevealEffectEnabled) return;

        _last = CreateHalo(e);
        PART_RippleCanvasRoot.Children.Add(_last);
        _pointers++;

        var (rippleVisual, opacityAnimation) = AnimationHelper.CreateAutoFinalEasingFloatAnimation(
            _last,
            AnimationHelper.Opacity);

        var implicitAnimation = rippleVisual.Compositor.CreateImplicitAnimationCollection();

        implicitAnimation[AnimationHelper.Opacity] = opacityAnimation;
        rippleVisual.ImplicitAnimations = implicitAnimation;

        _last.Opacity = 0.65;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        RemoveLastHalo();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_last is { } last)
        {
            var elementVisual = ElementComposition.GetElementVisual(last);
            if (elementVisual != null)
                elementVisual.ImplicitAnimations = null;
            PART_RippleCanvasRoot?.Children.Remove(last);
            _last = null;
            _pointers = 0;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void RemoveLastHalo()
    {
        var last = _last;

        if (last == null)
            return;

        if (_pointers > 0)
            _pointers--;

        OnReleaseHandler(last);
        _last = null;
    }

    private void OnReleaseHandler(Ellipse e)
    {
        // Fade out halo
        e.Opacity = 0;

        // Remove ripple from canvas to finalize ripple instance
        DispatcherTimer.RunOnce(RemoveRippleTask, Ripple.Duration);
        return;

        void RemoveRippleTask()
        {
            var elementVisual = ElementComposition.GetElementVisual(e);
            if (elementVisual != null)
                elementVisual.ImplicitAnimations = null;

            PART_RippleCanvasRoot?.Children.Remove(e);
        }
    }

    private Ellipse CreateHalo(PointerEventArgs e)
    {
        var maxSize = Math.Max(Bounds.Width, Bounds.Height) * 1.5;
        var p = e.GetPosition(PART_RippleCanvasRoot);

        var halo = new Ellipse
        {
            Width = maxSize,
            Height = maxSize,
            OpacityMask = MaskBrush,
            Fill = HaloBrush,
            Opacity = 0
        };

        Canvas.SetTop(halo, p.Y - maxSize / 2);
        Canvas.SetLeft(halo, p.X - maxSize / 2);

        return halo;
    }

    private void OnHaloBrushChanged()
    {
        if (_last == null) return;

        _last.Fill = HaloBrush;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == HaloBrushProperty)
            OnHaloBrushChanged();

        base.OnPropertyChanged(change);
    }
}
