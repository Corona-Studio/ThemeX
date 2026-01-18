using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ThemeX.Controls.Effects;
using ThemeX.Helpers;

namespace ThemeX.Controls.Animations;

public sealed class ActivatableToggle : AnimationLayer
{
    public static readonly StyledProperty<IBrush?> ActiveBrushProperty =
        AvaloniaProperty.Register<ActivatableToggle, IBrush?>(
            nameof(ActiveBrush), new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)));

    public static readonly StyledProperty<bool> ActivatedProperty = AvaloniaProperty.Register<ActivatableToggle, bool>(
        nameof(Activated));

    [Category("Animation")]
    public IBrush? ActiveBrush
    {
        get => GetValue(ActiveBrushProperty);
        set => SetValue(ActiveBrushProperty, value);
    }

    public bool Activated
    {
        get => GetValue(ActivatedProperty);
        set => SetValue(ActivatedProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(AnimationLayer);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        OnActivatedChanged();
    }

    protected override void BeginMouseEnterAnim(PointerEventArgs? e)
    {
        if (!MouseEnterAnimation) return;
        if (ActiveRect != null && !Activated)
            ActiveRect.Opacity = 1;
    }

    protected override void BeginMouseLeaveAnim(PointerEventArgs? e)
    {
        if (!MouseEnterAnimation) return;
        if (ActiveRect != null && !Activated)
            ActiveRect.Opacity = 0;
    }

    private void OnActivatedChanged()
    {
        if (ActiveRect == null)
            return;

        if (Activated)
        {
            ActiveRect.ClearValue(Shape.FillProperty);

            var path = ActiveBrush == null
                ? nameof(MouseOnBrush)
                : nameof(ActiveBrush);

            ActiveRect.CompiledBind(
                Shape.FillProperty,
                path,
                this,
                typeof(IBrush),
                o =>
                {
                    var that = (ActivatableToggle)o;

                    return that.ActiveBrush == null ? MouseOnBrush : ActiveBrush;
                });
        }
        else
        {
            ActiveRect.ClearValue(Shape.FillProperty);
            ActiveRect.CompiledBind(
                Shape.FillProperty,
                nameof(MouseOnBrush),
                this,
                typeof(Brush),
                o => ((ActivatableToggle)o).MouseOnBrush);
        }

        if (Activated)
            Task.Delay(Ripple.Duration)
                .ContinueWith(_ => Dispatcher.UIThread.Post(() => ActiveRect.Opacity = Activated ? 1 : 0));
        else
            ActiveRect.Opacity = 0;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ActivatedProperty)
            OnActivatedChanged();

        base.OnPropertyChanged(change);
    }
}