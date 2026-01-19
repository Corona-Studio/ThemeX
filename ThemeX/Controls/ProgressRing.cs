using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using ThemeX.SourceGenerator.Shared;

namespace ThemeX.Controls;

[PseudoClasses(":indeterminate")]
[PseudoClasses(":preserveaspect")]
public sealed partial class ProgressRing : RangeBase
{
    static ProgressRing()
    {
        MinimumProperty.Changed.AddClassHandler<ProgressRing>(OnMinimumPropertyChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnMaximumPropertyChanged);
        ValueProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnStartAnglePropertyChanged);
        MaximumProperty.Changed.AddClassHandler<ProgressRing>(OnEndAnglePropertyChanged);
    }

    public ProgressRing()
    {
        UpdatePseudoClasses(IsIndeterminate, PreserveAspect);
    }

    [StyledProp] public partial bool IsIndeterminate { get; set; }

    [StyledProp(DefaultValue = true)] public partial bool PreserveAspect { get; set; }

    [StyledProp] public partial double ValueAngle { get; set; }

    [StyledProp] public partial double StartAngle { get; set; }

    [StyledProp(DefaultValue = 360)] public partial double EndAngle { get; set; }

    partial void OnIsIndeterminateChanged()
    {
        UpdatePseudoClasses(IsIndeterminate, null);
    }

    partial void OnPreserveAspectChanged()
    {
        UpdatePseudoClasses(null, PreserveAspect);
    }

    private void UpdatePseudoClasses(
        bool? isIndeterminate,
        bool? preserveAspect)
    {
        if (isIndeterminate.HasValue) PseudoClasses.Set(":indeterminate", isIndeterminate.Value);

        if (preserveAspect.HasValue) PseudoClasses.Set(":preserveaspect", preserveAspect.Value);
    }

    private static void OnMinimumPropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.Minimum = (double)(e.NewValue ?? 0);
    }

    private static void OnMaximumPropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.Maximum = (double)(e.NewValue ?? 0);
    }

    private static void OnValuePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.ValueAngle = ((double)(e.NewValue ?? 0) - sender.Minimum) * (sender.EndAngle - sender.StartAngle) /
                            (sender.Maximum - sender.Minimum);
    }

    private static void OnStartAnglePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.StartAngle = (double)(e.NewValue ?? 0);
    }

    private static void OnEndAnglePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.EndAngle = (double)(e.NewValue ?? 0);
    }
}