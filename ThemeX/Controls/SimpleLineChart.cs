using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ThemeX.Controls;

public sealed class SimpleLineChart : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<double>?> DataSourceProperty =
        AvaloniaProperty.Register<SimpleLineChart, ObservableCollection<double>?>(
            nameof(DataSource));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<SimpleLineChart, double>(
            nameof(Maximum), 100);

    public static readonly StyledProperty<int> MaximumColumnsProperty =
        AvaloniaProperty.Register<SimpleLineChart, int>(
            nameof(MaximumColumns));

    public static readonly StyledProperty<IBrush> DataColumnFillProperty =
        AvaloniaProperty.Register<SimpleLineChart, IBrush>(
            nameof(DataColumnFill), new SolidColorBrush(Colors.Black));

    public static readonly StyledProperty<double> LineThicknessProperty =
        AvaloniaProperty.Register<SimpleLineChart, double>(
            nameof(LineThickness), 2);

    public ObservableCollection<double>? DataSource
    {
        get => GetValue(DataSourceProperty);
        set => SetValue(DataSourceProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int MaximumColumns
    {
        get => GetValue(MaximumColumnsProperty);
        set => SetValue(MaximumColumnsProperty, value);
    }

    public IBrush DataColumnFill
    {
        get => GetValue(DataColumnFillProperty);
        set => SetValue(DataColumnFillProperty, value);
    }

    public double LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    private void OnDataSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private static Color GetPrimaryColor(IBrush? brush)
    {
        return brush switch
        {
            ISolidColorBrush scb => scb.Color,
            IGradientBrush gb when gb.GradientStops.Count > 0 => gb.GradientStops[0].Color,
            _ => Colors.Black
        };
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0) return;

        var source = DataSource;
        if (source == null || source.Count == 0) return;

        var maxCols = MaximumColumns;
        var start = maxCols > 0 ? Math.Max(0, source.Count - maxCols) : 0;
        var count = source.Count - start;
        if (count <= 0) return;

        var maxValue = Maximum;
        if (maxValue <= 0) maxValue = 1;

        var points = new Point[count];
        for (var i = 0; i < count; i++)
        {
            var v = Math.Clamp(source[start + i] / maxValue, 0, 1);
            var x = count == 1 ? width * 0.5 : i * width / (count - 1);
            var y = height - v * height;
            points[i] = new Point(x, y);
        }

        // Gradient-filled area beneath the line: line color → transparent
        var lineColor = GetPrimaryColor(DataColumnFill);
        var areaGeom = new StreamGeometry();
        using (var ctx = areaGeom.Open())
        {
            ctx.BeginFigure(new Point(points[0].X, height));
            foreach (var p in points)
                ctx.LineTo(p);
            ctx.LineTo(new Point(points[^1].X, height));
            ctx.EndFigure(true);
        }

        var areaFill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(lineColor, 0),
                new GradientStop(Color.FromArgb(0, lineColor.R, lineColor.G, lineColor.B), 1)
            ]
        };

        context.DrawGeometry(areaFill, null, areaGeom);

        // Line stroke
        if (points.Length > 1)
        {
            var pen = new Pen(DataColumnFill, LineThickness,
                lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            var lineGeom = new StreamGeometry();
            using (var ctx = lineGeom.Open())
            {
                ctx.BeginFigure(points[0], false);
                for (var i = 1; i < points.Length; i++)
                    ctx.LineTo(points[i]);
                ctx.EndFigure(false);
            }

            context.DrawGeometry(null, pen, lineGeom);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataSourceProperty)
        {
            if (change.OldValue is ObservableCollection<double> old)
                old.CollectionChanged -= OnDataSourceCollectionChanged;
            if (change.NewValue is ObservableCollection<double> @new)
                @new.CollectionChanged += OnDataSourceCollectionChanged;
            InvalidateVisual();
        }
        else if (change.Property == MaximumProperty
                 || change.Property == MaximumColumnsProperty
                 || change.Property == LineThicknessProperty
                 || change.Property == DataColumnFillProperty)
        {
            InvalidateVisual();
        }
    }
}