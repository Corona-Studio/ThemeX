using Avalonia;
using Avalonia.Controls;

namespace ThemeX.Controls.Scroll;

public sealed partial class SmoothScrollViewer : ScrollViewer
{
    protected override Type StyleKeyOverride { get; } = typeof(SmoothScrollViewer);

    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool UseSmoothScrolling { get; set; }

    [GeneratedStyledProperty(DefaultValue = 200d)]
    public partial double SmoothScrollingStep { get; set; }

    [GeneratedStyledProperty]
    public partial bool UseCustomScrollAnimation { get; set; }
}