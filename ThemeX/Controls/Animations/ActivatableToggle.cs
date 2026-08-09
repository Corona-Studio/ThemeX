using System.ComponentModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ThemeX.Controls.Effects;

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
            ActiveRect.Bind(Shape.FillProperty,
                ActiveBrush == null
                    ? this.GetObservable(MouseOnBrushProperty)
                    : this.GetObservable(ActiveBrushProperty).Select(v => v ?? MouseOnBrush));
        }
        else
        {
            ActiveRect.ClearValue(Shape.FillProperty);
            ActiveRect.Bind(Shape.FillProperty, this.GetObservable(MouseOnBrushProperty));
        }

        if (Activated)
            DispatcherTimer.RunOnce(() => ActiveRect.Opacity = Activated ? 1 : 0, Ripple.Duration);
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
