using System.Collections.Frozen;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace ThemeX;

public sealed class ColorPresets : ResourceProvider
{
    public static FrozenDictionary<string, ImmutableSolidColorBrush> Colors { get; } =
        new Dictionary<string, ImmutableSolidColorBrush>
        {
            { "LauncherX.ColorPicker.Preset1", new ImmutableSolidColorBrush(Color.Parse("#FFB900")) },
            { "LauncherX.ColorPicker.Preset2", new ImmutableSolidColorBrush(Color.Parse("#ff8c00")) },
            { "LauncherX.ColorPicker.Preset3", new ImmutableSolidColorBrush(Color.Parse("#f7630c")) },
            { "LauncherX.ColorPicker.Preset4", new ImmutableSolidColorBrush(Color.Parse("#ca5010")) },
            { "LauncherX.ColorPicker.Preset5", new ImmutableSolidColorBrush(Color.Parse("#da3b01")) },
            { "LauncherX.ColorPicker.Preset6", new ImmutableSolidColorBrush(Color.Parse("#ef6950")) },
            { "LauncherX.ColorPicker.Preset7", new ImmutableSolidColorBrush(Color.Parse("#d13438")) },
            { "LauncherX.ColorPicker.Preset8", new ImmutableSolidColorBrush(Color.Parse("#ff4343")) },
            { "LauncherX.ColorPicker.Preset9", new ImmutableSolidColorBrush(Color.Parse("#e74856")) },
            { "LauncherX.ColorPicker.Preset10", new ImmutableSolidColorBrush(Color.Parse("#e81123")) },
            { "LauncherX.ColorPicker.Preset11", new ImmutableSolidColorBrush(Color.Parse("#ea005e")) },
            { "LauncherX.ColorPicker.Preset12", new ImmutableSolidColorBrush(Color.Parse("#c30052")) },
            { "LauncherX.ColorPicker.Preset13", new ImmutableSolidColorBrush(Color.Parse("#e3008c")) },
            { "LauncherX.ColorPicker.Preset14", new ImmutableSolidColorBrush(Color.Parse("#bf0077")) },
            { "LauncherX.ColorPicker.Preset15", new ImmutableSolidColorBrush(Color.Parse("#c239b3")) },
            { "LauncherX.ColorPicker.Preset16", new ImmutableSolidColorBrush(Color.Parse("#9a0089")) },
            { "LauncherX.ColorPicker.Preset17", new ImmutableSolidColorBrush(Color.Parse("#0078d7")) },
            { "LauncherX.ColorPicker.Preset18", new ImmutableSolidColorBrush(Color.Parse("#0063b1")) },
            { "LauncherX.ColorPicker.Preset19", new ImmutableSolidColorBrush(Color.Parse("#8e8cd8")) },
            { "LauncherX.ColorPicker.Preset20", new ImmutableSolidColorBrush(Color.Parse("#6b69d6")) },
            { "LauncherX.ColorPicker.Preset21", new ImmutableSolidColorBrush(Color.Parse("#8764b8")) },
            { "LauncherX.ColorPicker.Preset22", new ImmutableSolidColorBrush(Color.Parse("#744da9")) },
            { "LauncherX.ColorPicker.Preset23", new ImmutableSolidColorBrush(Color.Parse("#b146c2")) },
            { "LauncherX.ColorPicker.Preset24", new ImmutableSolidColorBrush(Color.Parse("#881798")) },
            { "LauncherX.ColorPicker.Preset25", new ImmutableSolidColorBrush(Color.Parse("#0099bc")) },
            { "LauncherX.ColorPicker.Preset26", new ImmutableSolidColorBrush(Color.Parse("#2d7d9a")) },
            { "LauncherX.ColorPicker.Preset27", new ImmutableSolidColorBrush(Color.Parse("#00b7c3")) },
            { "LauncherX.ColorPicker.Preset28", new ImmutableSolidColorBrush(Color.Parse("#038387")) },
            { "LauncherX.ColorPicker.Preset29", new ImmutableSolidColorBrush(Color.Parse("#00b294")) },
            { "LauncherX.ColorPicker.Preset30", new ImmutableSolidColorBrush(Color.Parse("#018574")) },
            { "LauncherX.ColorPicker.Preset31", new ImmutableSolidColorBrush(Color.Parse("#00cc6a")) },
            { "LauncherX.ColorPicker.Preset32", new ImmutableSolidColorBrush(Color.Parse("#10893e")) },
            { "LauncherX.ColorPicker.Preset33", new ImmutableSolidColorBrush(Color.Parse("#7a7574")) },
            { "LauncherX.ColorPicker.Preset34", new ImmutableSolidColorBrush(Color.Parse("#5d5a58")) },
            { "LauncherX.ColorPicker.Preset35", new ImmutableSolidColorBrush(Color.Parse("#68768a")) },
            { "LauncherX.ColorPicker.Preset36", new ImmutableSolidColorBrush(Color.Parse("#515c6b")) },
            { "LauncherX.ColorPicker.Preset37", new ImmutableSolidColorBrush(Color.Parse("#567c73")) },
            { "LauncherX.ColorPicker.Preset38", new ImmutableSolidColorBrush(Color.Parse("#486860")) },
            { "LauncherX.ColorPicker.Preset39", new ImmutableSolidColorBrush(Color.Parse("#498205")) },
            { "LauncherX.ColorPicker.Preset40", new ImmutableSolidColorBrush(Color.Parse("#107c10")) },
            { "LauncherX.ColorPicker.Preset41", new ImmutableSolidColorBrush(Color.Parse("#767676")) },
            { "LauncherX.ColorPicker.Preset42", new ImmutableSolidColorBrush(Color.Parse("#4c4a48")) },
            { "LauncherX.ColorPicker.Preset43", new ImmutableSolidColorBrush(Color.Parse("#69797e")) },
            { "LauncherX.ColorPicker.Preset44", new ImmutableSolidColorBrush(Color.Parse("#4a5459")) },
            { "LauncherX.ColorPicker.Preset45", new ImmutableSolidColorBrush(Color.Parse("#647c64")) },
            { "LauncherX.ColorPicker.Preset46", new ImmutableSolidColorBrush(Color.Parse("#525e54")) },
            { "LauncherX.ColorPicker.Preset47", new ImmutableSolidColorBrush(Color.Parse("#847545")) },
            { "LauncherX.ColorPicker.Preset48", new ImmutableSolidColorBrush(Color.Parse("#7e735f")) }
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