using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using ThemeX.Helpers;

namespace ThemeX.Controls.Effects;

public sealed class Ripple : Ellipse
{
    public static readonly TimeSpan Duration = new(0, 0, 0, 0, 450);
    private readonly double _endX;
    private readonly double _endY;

    private readonly double _maxDiam;

    public Ripple(double outerWidth, double outerHeight)
    {
        Width = 0;
        Height = 0;

        _maxDiam = Math.Sqrt(Math.Pow(outerWidth, 2) + Math.Pow(outerHeight, 2));
        _endY = _maxDiam - outerHeight;
        _endX = _maxDiam - outerWidth;

        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        Opacity = 1;
    }

    public void SetupInitialValues(PointerPressedEventArgs e, Control parent)
    {
        var pointer = e.GetPosition(parent);
        Margin = new Thickness(pointer.X, pointer.Y, 0, 0);
    }

    public void RunFirstStep()
    {
        var (rippleVisual, scaleAnimation) = AnimationHelper.CreateCompositionVector3Animation(
            this,
            Vector3.One,
            Vector3.One * (float)_maxDiam,
            Duration);

        rippleVisual.StartAnimation(AnimationHelper.Scale, scaleAnimation);

        Margin = new Thickness(-_endX / 2, -_endY / 2, 0, 0);
    }

    public void RunSecondStep()
    {
        if (!IsLoaded) return;

        var (rippleVisual, opacityAnimation) = AnimationHelper.CreateCompositionFloatAnimation(
            this,
            1f,
            0f,
            Duration);

        rippleVisual.StartAnimation(AnimationHelper.Opacity, opacityAnimation);
    }
}