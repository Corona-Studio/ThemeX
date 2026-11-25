using System.ComponentModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace ThemeX.Controls.Animations;

public sealed partial class ActivatableToggle : AnimationLayer
{
    public static readonly StyledProperty<IBrush?> ActiveBrushProperty =
        AvaloniaProperty.Register<ActivatableToggle, IBrush?>(
            nameof(ActiveBrush),
            new ImmutableSolidColorBrush(Color.FromArgb(64, 0, 0, 0)));

    [Category("Animation")]
    public IBrush? ActiveBrush
    {
        get => GetValue(ActiveBrushProperty);
        set => SetValue(ActiveBrushProperty, value);
    }

    [GeneratedStyledProperty]
    public partial bool Activated { get; set; }

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
            ActiveRect.Opacity = 1;
        else
            ActiveRect.Opacity = IsPointerOver && MouseEnterAnimation ? 1 : 0;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ActivatedProperty)
            OnActivatedChanged();

        base.OnPropertyChanged(change);
    }
}
