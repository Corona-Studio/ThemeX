using Avalonia.Media;

namespace ThemeX.Models;

public record struct QuantizedColor(Color Color, int Population)
{
    public bool IsDark { get; } = CalculateYiqLuma(Color) < 128;

    private static int CalculateYiqLuma(Color color)
    {
        return Convert.ToInt32(Math.Round((299 * color.R + 587 * color.G + 114 * color.B) / 1000f));
    }
}