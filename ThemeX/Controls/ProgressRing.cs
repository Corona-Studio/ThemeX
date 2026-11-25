using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace ThemeX.Controls;

[PseudoClasses(":indeterminate")]
[PseudoClasses(":preserveaspect")]
public sealed partial class ProgressRing : RangeBase
{
    public ProgressRing()
    {
        UpdatePseudoClasses(IsIndeterminate, PreserveAspect);
    }

    [GeneratedStyledProperty]
    public partial bool IsIndeterminate { get; set; }

    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool PreserveAspect { get; set; }

    [GeneratedStyledProperty]
    public partial double ValueAngle { get; set; }

    [GeneratedStyledProperty]
    public partial double StartAngle { get; set; }

    [GeneratedStyledProperty(DefaultValue = 360d)]
    public partial double EndAngle { get; set; }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsIndeterminateProperty)
            UpdatePseudoClasses(IsIndeterminate, null);
        else if (change.Property == PreserveAspectProperty)
            UpdatePseudoClasses(null, PreserveAspect);
        else if (change.Property == MinimumProperty ||
                 change.Property == MaximumProperty ||
                 change.Property == ValueProperty ||
                 change.Property == StartAngleProperty ||
                 change.Property == EndAngleProperty)
            UpdateValueAngle();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Infinite style animations are stopped when the control leaves the visual tree.
        // Restore the pseudo class on reattachment so a reused ProgressRing starts again.
        UpdatePseudoClasses(IsIndeterminate, PreserveAspect);
        UpdateValueAngle();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Make the indeterminate selector transition again on the next attachment.
        UpdatePseudoClasses(false, null);

        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateValueAngle()
    {
        var span = Maximum - Minimum;
        ValueAngle = span <= 0
            ? 0
            : (Value - Minimum) * (EndAngle - StartAngle) / span;
    }

    private void UpdatePseudoClasses(
        bool? isIndeterminate,
        bool? preserveAspect)
    {
        if (isIndeterminate.HasValue) PseudoClasses.Set(":indeterminate", isIndeterminate.Value);

        if (preserveAspect.HasValue) PseudoClasses.Set(":preserveaspect", preserveAspect.Value);
    }
}
