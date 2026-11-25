using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ThemeX.Controls;

/// <summary>
///     Fluent Design 2 card/container with built-in mouse-tracking proximity Reveal border light
///     and surface hover spotlight.
/// </summary>
public partial class RevealBorder : ContentControl
{
    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool IsRevealEnabled { get; set; }

    [GeneratedStyledProperty(DefaultValue = RevealHighlightMode.BorderAndHover)]
    public partial RevealHighlightMode RevealMode { get; set; }

    [GeneratedStyledProperty(DefaultValue = RevealHighlightTone.Standard)]
    public partial RevealHighlightTone RevealTone { get; set; }

    [GeneratedStyledProperty(DefaultValue = 84d)]
    public partial double BorderLightRadius { get; set; }

    [GeneratedStyledProperty(DefaultValue = 112d)]
    public partial double HoverLightRadius { get; set; }

    [GeneratedStyledProperty(DefaultValue = 1.0d)]
    public partial double RevealBorderThickness { get; set; }

    [GeneratedStyledProperty]
    public partial BoxShadows BoxShadow { get; set; }

    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool ClipContent { get; set; }
}
