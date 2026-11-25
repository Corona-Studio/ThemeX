using Avalonia;
using Avalonia.Media;

namespace ThemeX.Helpers;

public static class GeometryHelper
{
    private const double PiOver2 = Math.PI / 2.0; // 90 deg to rad

    extension(CornerRadius cornerRadius)
    {
        public double GetAverageCornerRadius()
        {
            return (cornerRadius.TopLeft + cornerRadius.TopRight + cornerRadius.BottomLeft + cornerRadius.BottomRight) / 4;
        }
    }

    public static Geometry CreateDefiningGeometry(Size size, double thickness = 0, double radiusX = 0,
        double radiusY = 0)
    {
        if (radiusX == 0 && radiusY == 0)
        {
            // Optimization when there are no corner radii
            var rect = new Rect(size).Deflate(thickness / 2);
            return new RectangleGeometry(rect);
        }
        else
        {
            var rect = new Rect(size).Deflate(thickness / 2);
            var geometry = new StreamGeometry();
            var arcSize = new Size(radiusX, radiusY);

            using var context = geometry.Open();

            // The rectangle is constructed as follows:
            //
            //   (origin)
            //   Corner 4            Corner 1
            //   Top/Left  Line 1    Top/Right
            //      \_   __________   _/
            //          |          |
            //   Line 4 |          | Line 2
            //       _  |__________|  _
            //      /      Line 3      \
            //   Corner 3            Corner 2
            //   Bottom/Left         Bottom/Right
            //
            // - Lines 1,3 follow the deflated rectangle bounds minus RadiusX
            // - Lines 2,4 follow the deflated rectangle bounds minus RadiusY
            // - All corners are constructed using elliptical arcs 

            // Line 1 + Corner 1
            context.BeginFigure(new Point(rect.Left + radiusX, rect.Top));
            context.LineTo(new Point(rect.Right - radiusX, rect.Top));
            context.ArcTo(
                new Point(rect.Right, rect.Top + radiusY),
                arcSize,
                PiOver2,
                false,
                SweepDirection.Clockwise);

            // Line 2 + Corner 2
            context.LineTo(new Point(rect.Right, rect.Bottom - radiusY));
            context.ArcTo(
                new Point(rect.Right - radiusX, rect.Bottom),
                arcSize,
                PiOver2,
                false,
                SweepDirection.Clockwise);

            // Line 3 + Corner 3
            context.LineTo(new Point(rect.Left + radiusX, rect.Bottom));
            context.ArcTo(
                new Point(rect.Left, rect.Bottom - radiusY),
                arcSize,
                PiOver2,
                false,
                SweepDirection.Clockwise);

            // Line 4 + Corner 4
            context.LineTo(new Point(rect.Left, rect.Top + radiusY));
            context.ArcTo(
                new Point(rect.Left + radiusX, rect.Top),
                arcSize,
                PiOver2,
                false,
                SweepDirection.Clockwise);

            context.EndFigure(true);

            return geometry;
        }
    }

    /// <summary>
    ///     Builds an exact rounded-rectangle clip geometry matching <paramref name="size" /> and
    ///     <paramref name="cornerRadius" /> (with CSS/Skia proportional corner-radius clamping so
    ///     large values like pill radii never invert). Returns <see langword="null" /> when the size
    ///     is empty or all corner radii are zero so callers can clear <see cref="Visual.Clip" />.
    /// </summary>
    public static Geometry? CreateRoundedClipGeometry(Size size, CornerRadius cornerRadius)
    {
        var width = size.Width;
        var height = size.Height;
        if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0)
            return null;

        var tl = SanitizeRadius(cornerRadius.TopLeft);
        var tr = SanitizeRadius(cornerRadius.TopRight);
        var br = SanitizeRadius(cornerRadius.BottomRight);
        var bl = SanitizeRadius(cornerRadius.BottomLeft);

        if (tl <= 0 && tr <= 0 && br <= 0 && bl <= 0)
            return null;

        // Proportionally clamp radii so adjacent corner arcs never overlap (matches Skia SkRRect).
        var scale = 1.0;

        var topSum = tl + tr;
        if (topSum > width)
            scale = Math.Min(scale, width / topSum);

        var bottomSum = bl + br;
        if (bottomSum > width)
            scale = Math.Min(scale, width / bottomSum);

        var leftSum = tl + bl;
        if (leftSum > height)
            scale = Math.Min(scale, height / leftSum);

        var rightSum = tr + br;
        if (rightSum > height)
            scale = Math.Min(scale, height / rightSum);

        if (scale < 1.0)
        {
            tl *= scale;
            tr *= scale;
            br *= scale;
            bl *= scale;
        }

        // Fast native SkRRect path for uniform corner radii.
        if (tl == tr && tr == br && br == bl)
            return new RectangleGeometry(new Rect(0, 0, width, height), tl, tl);

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new Point(tl, 0));

            var topEnd = width - tr;
            if (topEnd > tl)
                context.LineTo(new Point(topEnd, 0));

            if (tr > 0)
                context.ArcTo(new Point(width, tr), new Size(tr, tr), 0, false, SweepDirection.Clockwise);

            var rightEnd = height - br;
            if (rightEnd > tr)
                context.LineTo(new Point(width, rightEnd));

            if (br > 0)
                context.ArcTo(new Point(width - br, height), new Size(br, br), 0, false, SweepDirection.Clockwise);

            if (width - br > bl)
                context.LineTo(new Point(bl, height));

            if (bl > 0)
                context.ArcTo(new Point(0, height - bl), new Size(bl, bl), 0, false, SweepDirection.Clockwise);

            if (height - bl > tl)
                context.LineTo(new Point(0, tl));

            if (tl > 0)
                context.ArcTo(new Point(tl, 0), new Size(tl, tl), 0, false, SweepDirection.Clockwise);

            context.EndFigure(true);
        }

        return geometry;

        static double SanitizeRadius(double radius) => double.IsFinite(radius) && radius > 0 ? radius : 0;
    }
}
