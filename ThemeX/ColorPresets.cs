using System.Collections.Frozen;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace ThemeX;

public sealed class ColorPresets : ResourceProvider
{
    public static string GetPaletteKey(int index) => $"ThemeX.ColorPalette{index}";

    public static FrozenDictionary<string, ImmutableSolidColorBrush> Colors { get; } =
        new Dictionary<string, ImmutableSolidColorBrush>
        {
            { "ThemeX.ColorPalette1", new ImmutableSolidColorBrush(Color.Parse("#FFB900")) },
            { "ThemeX.ColorPalette2", new ImmutableSolidColorBrush(Color.Parse("#ff8c00")) },
            { "ThemeX.ColorPalette3", new ImmutableSolidColorBrush(Color.Parse("#f7630c")) },
            { "ThemeX.ColorPalette4", new ImmutableSolidColorBrush(Color.Parse("#ca5010")) },
            { "ThemeX.ColorPalette5", new ImmutableSolidColorBrush(Color.Parse("#da3b01")) },
            { "ThemeX.ColorPalette6", new ImmutableSolidColorBrush(Color.Parse("#ef6950")) },
            { "ThemeX.ColorPalette7", new ImmutableSolidColorBrush(Color.Parse("#d13438")) },
            { "ThemeX.ColorPalette8", new ImmutableSolidColorBrush(Color.Parse("#ff4343")) },
            { "ThemeX.ColorPalette9", new ImmutableSolidColorBrush(Color.Parse("#e74856")) },
            { "ThemeX.ColorPalette10", new ImmutableSolidColorBrush(Color.Parse("#e81123")) },
            { "ThemeX.ColorPalette11", new ImmutableSolidColorBrush(Color.Parse("#ea005e")) },
            { "ThemeX.ColorPalette12", new ImmutableSolidColorBrush(Color.Parse("#c30052")) },
            { "ThemeX.ColorPalette13", new ImmutableSolidColorBrush(Color.Parse("#e3008c")) },
            { "ThemeX.ColorPalette14", new ImmutableSolidColorBrush(Color.Parse("#bf0077")) },
            { "ThemeX.ColorPalette15", new ImmutableSolidColorBrush(Color.Parse("#c239b3")) },
            { "ThemeX.ColorPalette16", new ImmutableSolidColorBrush(Color.Parse("#9a0089")) },
            { "ThemeX.ColorPalette17", new ImmutableSolidColorBrush(Color.Parse("#0078d7")) },
            { "ThemeX.ColorPalette18", new ImmutableSolidColorBrush(Color.Parse("#0063b1")) },
            { "ThemeX.ColorPalette19", new ImmutableSolidColorBrush(Color.Parse("#8e8cd8")) },
            { "ThemeX.ColorPalette20", new ImmutableSolidColorBrush(Color.Parse("#6b69d6")) },
            { "ThemeX.ColorPalette21", new ImmutableSolidColorBrush(Color.Parse("#8764b8")) },
            { "ThemeX.ColorPalette22", new ImmutableSolidColorBrush(Color.Parse("#744da9")) },
            { "ThemeX.ColorPalette23", new ImmutableSolidColorBrush(Color.Parse("#b146c2")) },
            { "ThemeX.ColorPalette24", new ImmutableSolidColorBrush(Color.Parse("#881798")) },
            { "ThemeX.ColorPalette25", new ImmutableSolidColorBrush(Color.Parse("#0099bc")) },
            { "ThemeX.ColorPalette26", new ImmutableSolidColorBrush(Color.Parse("#2d7d9a")) },
            { "ThemeX.ColorPalette27", new ImmutableSolidColorBrush(Color.Parse("#00b7c3")) },
            { "ThemeX.ColorPalette28", new ImmutableSolidColorBrush(Color.Parse("#038387")) },
            { "ThemeX.ColorPalette29", new ImmutableSolidColorBrush(Color.Parse("#00b294")) },
            { "ThemeX.ColorPalette30", new ImmutableSolidColorBrush(Color.Parse("#018574")) },
            { "ThemeX.ColorPalette31", new ImmutableSolidColorBrush(Color.Parse("#00cc6a")) },
            { "ThemeX.ColorPalette32", new ImmutableSolidColorBrush(Color.Parse("#10893e")) },
            { "ThemeX.ColorPalette33", new ImmutableSolidColorBrush(Color.Parse("#7a7574")) },
            { "ThemeX.ColorPalette34", new ImmutableSolidColorBrush(Color.Parse("#5d5a58")) },
            { "ThemeX.ColorPalette35", new ImmutableSolidColorBrush(Color.Parse("#68768a")) },
            { "ThemeX.ColorPalette36", new ImmutableSolidColorBrush(Color.Parse("#515c6b")) },
            { "ThemeX.ColorPalette37", new ImmutableSolidColorBrush(Color.Parse("#567c73")) },
            { "ThemeX.ColorPalette38", new ImmutableSolidColorBrush(Color.Parse("#486860")) },
            { "ThemeX.ColorPalette39", new ImmutableSolidColorBrush(Color.Parse("#498205")) },
            { "ThemeX.ColorPalette40", new ImmutableSolidColorBrush(Color.Parse("#107c10")) },
            { "ThemeX.ColorPalette41", new ImmutableSolidColorBrush(Color.Parse("#767676")) },
            { "ThemeX.ColorPalette42", new ImmutableSolidColorBrush(Color.Parse("#4c4a48")) },
            { "ThemeX.ColorPalette43", new ImmutableSolidColorBrush(Color.Parse("#69797e")) },
            { "ThemeX.ColorPalette44", new ImmutableSolidColorBrush(Color.Parse("#4a5459")) },
            { "ThemeX.ColorPalette45", new ImmutableSolidColorBrush(Color.Parse("#647c64")) },
            { "ThemeX.ColorPalette46", new ImmutableSolidColorBrush(Color.Parse("#525e54")) },
            { "ThemeX.ColorPalette47", new ImmutableSolidColorBrush(Color.Parse("#847545")) },
            { "ThemeX.ColorPalette48", new ImmutableSolidColorBrush(Color.Parse("#7e735f")) }
        }.ToFrozenDictionary();

    public override bool HasResources => true;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (key is string strKey)
        {
            value = Colors.GetValueOrDefault(strKey);
            return value != null;
        }

        value = null;
        return false;
    }
}