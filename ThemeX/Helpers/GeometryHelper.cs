using Avalonia;
using Avalonia.Media;

namespace ThemeX.Helpers;

public static class GeometryHelper
{
    private const double PiOver2 = Math.PI / 2.0; // 90 deg to rad

    public static double GetAverageCornerRadius(this CornerRadius cornerRadius)
    {
        return (cornerRadius.TopLeft + cornerRadius.TopRight + cornerRadius.BottomLeft + cornerRadius.BottomRight) / 4;
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
}