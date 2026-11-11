using Avalonia.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using ThemeX.Models;
using Color = Avalonia.Media.Color;

namespace ThemeX.Helpers;

public static class ColorHelper
{
    public static bool IsImageDark(Image<Rgba32> image, float threshold = 0.5f, int sampleSize = 64)
    {
        // Resize the image to speed up processing
        using var resized = image.Clone(ctx =>
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(sampleSize, sampleSize),
                Mode = ResizeMode.Stretch
            }));

        var totalBrightness = 0d;
        var pixelCount = resized.Width * resized.Height;

        resized.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                foreach (var pixel in row)
                {
                    var r = pixel.R / 255f;
                    var g = pixel.G / 255f;
                    var b = pixel.B / 255f;

                    // Perceived luminance formula
                    totalBrightness += 0.2126 * r + 0.7152 * g + 0.0722 * b;
                }
            }
        });

        var average = totalBrightness / pixelCount;
        return average < threshold;
    }

    extension(Image<Rgba32> bitmap)
    {
        /// <summary>
        ///     获取图片主题颜色
        /// </summary>
        /// <param name="colorCount">颜色数量</param>
        /// <returns></returns>
        public IEnumerable<QuantizedColor> GetPaletteFromBitmap(int colorCount = 8)
        {
            var quantizedImage = bitmap.Clone(ctx =>
            {
                ctx.Resize(256, 0)
                    .Quantize(new HexadecatreeQuantizer(new QuantizerOptions
                    {
                        MaxColors = colorCount + 3
                    }));
            });

            var colors = new Dictionary<Rgba32, int>();

            for (var x = 0; x < quantizedImage.Width; x++)
            for (var y = 0; y < quantizedImage.Height; y++)
            {
                var col = quantizedImage[x, y];

                if (!colors.TryAdd(col, 1))
                    colors[col]++;
            }

            var colorList = colors
                .OrderByDescending(c => c.Value)
                .Where(c => c.Key is not { A: 255, R: 255, G: 255, B: 255 })
                .Take(colorCount)
                .Select(c => new QuantizedColor(new Color(c.Key.A, c.Key.R, c.Key.G, c.Key.B), c.Value))
                .ToList();

            return colorList;
        }
    }

    extension(Color color)
    {
        /// <summary>
        ///     获取一个颜色的人眼感知亮度，并以 0~1 之间的小数表示。
        /// </summary>
        public double GetGrayLevel()
        {
            return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
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