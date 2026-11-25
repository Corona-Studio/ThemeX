using Avalonia.Media;
using Color = Avalonia.Media.Color;

namespace ThemeX.Helpers;

public static class ColorHelper
{
    extension(Color color)
    {
        /// <summary>
        ///     获取一个颜色的人眼感知亮度，并以 0~1 之间的小数表示。
        /// </summary>
        public double GetGrayLevel()
        {
            return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        }

        /// <summary>
        ///     获取一个颜色在指定背景色上按 Alpha 混合后的人眼感知亮度，并以 0~1 之间的小数表示。
        /// </summary>
        public double GetGrayLevel(Color backdrop)
        {
            if (color.A == 255)
                return color.GetGrayLevel();

            var alpha = color.A / 255.0;
            var invAlpha = 1.0 - alpha;
            var r = color.R * alpha + backdrop.R * invAlpha;
            var g = color.G * alpha + backdrop.G * invAlpha;
            var b = color.B * alpha + backdrop.B * invAlpha;

            return (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
        }

        /// <summary>
        ///     计算颜色的 WCAG 2.1 相对亮度（0~1）。
        /// </summary>
        public double GetRelativeLuminance()
        {
            static double ToLinear(byte channel)
            {
                var c = channel / 255.0;
                return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * ToLinear(color.R) + 0.7152 * ToLinear(color.G) + 0.0722 * ToLinear(color.B);
        }

        /// <summary>
        ///     计算两个颜色之间的 WCAG 2.1 对比度（1~21）。
        /// </summary>
        public double GetContrastRatio(Color other)
        {
            var l1 = color.GetRelativeLuminance();
            var l2 = other.GetRelativeLuminance();
            var lighter = Math.Max(l1, l2);
            var darker = Math.Min(l1, l2);
            return (lighter + 0.05) / (darker + 0.05);
        }

        public Color ToMoreTransparent()
        {
            var alpha = color.A * 0.8;

            return Color.FromArgb((byte)alpha, color.R, color.G, color.B);
        }

        public Color ToMoreTransparent(int times)
        {
            for (var i = 0; i < times; i++)
                color = color.ToMoreTransparent();

            return color;
        }

        public Color ToLighter()
        {
            var hsl = color.ToHsl();

            return new HslColor(hsl.A, hsl.H, hsl.S, hsl.L * 1.25).ToRgb();
        }

        public Color ToDarker()
        {
            var hsl = color.ToHsl();

            return new HslColor(hsl.A, hsl.H, hsl.S, hsl.L * 0.75).ToRgb();
        }
    }
}