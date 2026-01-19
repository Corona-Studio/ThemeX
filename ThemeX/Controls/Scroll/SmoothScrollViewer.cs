using Avalonia.Controls;
using ThemeX.SourceGenerator.Shared;

namespace ThemeX.Controls.Scroll;

public sealed partial class SmoothScrollViewer : ScrollViewer
{
    protected override Type StyleKeyOverride { get; } = typeof(SmoothScrollViewer);

    [StyledProp(DefaultValue = true)] public partial bool UseSmoothScrolling { get; set; }

    [StyledProp(DefaultValue = 200d)] public partial double SmoothScrollingStep { get; set; }

    [StyledProp] public partial bool UseCustomScrollAnimation { get; set; }
}