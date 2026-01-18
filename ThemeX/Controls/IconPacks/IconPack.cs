using Avalonia.Controls.Primitives;
using LauncherX.SourceGenerator.Shared;

namespace ThemeX.Controls.IconPacks;

public sealed partial class IconPack : TemplatedControl
{
    [StyledProp] public partial string Symbol { get; set; }
}