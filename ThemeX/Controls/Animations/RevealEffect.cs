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

public sealed class RevealEffect : AnimationLayer
{
    public static readonly StyledProperty<ISolidColorBrush> HaloBrushProperty =
        AvaloniaProperty.Register<RevealEffect, ISolidColorBrush>(
            nameof(HaloBrush));

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

    public ISolidColorBrush HaloBrush
    {
        get => GetValue(HaloBrushProperty);
        set => SetValue(HaloBrushProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(AnimationLayer);

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_last == null) return;

        var p = e.GetPosition(PART_RippleCanvasRoot);

        Canvas.SetTop(_last, p.Y - _last.Bounds.Height / 2);
        Canvas.SetLeft(_last, p.X - _last.Bounds.Width / 2);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_pointers != 0) return;

        _last = CreateHalo(e);
        PART_RippleCanvasRoot!.Children.Add(_last);
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

    private void RemoveLastHalo()
    {
        var last = _last;

        if (last == null)
            return;

        _pointers--;

        // This way to handle pointer released is pretty tricky
        // could have better way to improve
        OnReleaseHandler(last);
        _last = null;
    }

    private void OnReleaseHandler(Ellipse e)
    {
        // Fade out halo
        e.Opacity = 0;

        // Remove ripple from canvas to finalize ripple instance
        Task.Delay(Ripple.Duration)
            .ContinueWith(_ => Dispatcher.UIThread.Post(RemoveRippleTask));
        return;

        void RemoveRippleTask()
        {
            var elementVisual = ElementComposition.GetElementVisual(e);

            elementVisual?.ImplicitAnimations = null;

            PART_RippleCanvasRoot!.Children.Remove(e);
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

        Canvas.SetTop(halo, p.Y - halo.Bounds.Height / 2);
        Canvas.SetLeft(halo, p.X - halo.Bounds.Width / 2);

        return halo;
    }

    private void OnHaloBrushChanged()
    {
        if (_last == null) return;

        _last!.Fill = HaloBrush;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == HaloBrushProperty)
            OnHaloBrushChanged();

        base.OnPropertyChanged(change);
    }
}