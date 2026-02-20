using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;

namespace ThemeX.Controls;

[TemplatePart("MainGrid", typeof(Grid), IsRequired = true)]
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

    private readonly List<Rectangle> _columnShapes = [];
    private readonly Lock _locker = new();
    private Grid? _mainGrid;
    private int _currentColumnCount;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _mainGrid = e.NameScope.Find<Grid>("MainGrid")!;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        OnDataSourceChanged();
    }

    public ObservableCollection<double>? DataSource
    {
        get => GetValue(DataSourceProperty);
        set
        {
            var old = GetValue(DataSourceProperty);

            if (old != null)
                old.CollectionChanged -= DataSourceOnCollectionChanged;

            SetValue(DataSourceProperty, value);
        }
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        SizeChanged += OnSizeChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        SizeChanged -= OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        OnMaximumChanged();
    }

    private void DataSourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            lock (_locker)
            {
                _mainGrid!.Children.Clear();
                _columnShapes.Clear();
            }


            return;
        }

        if (e.Action == NotifyCollectionChangedAction.Replace)
        {
            lock (_locker)
            {
                _mainGrid!.Children.Remove(_columnShapes[e.OldStartingIndex]);
            }


            var newRect = CreateNewRect(DataSource![e.OldStartingIndex]);

            lock (_locker)
            {
                _columnShapes[e.OldStartingIndex] = newRect;
            }

            Grid.SetColumn(newRect, e.OldStartingIndex);

            _mainGrid.Children.Add(newRect);
        }

        if (e.Action is NotifyCollectionChangedAction.Move)
        {
            OnDataSourceChanged();
            return;
        }

        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldStartingIndex < MaximumColumns - _currentColumnCount)
                return;

            lock (_locker)
            {
                var oldRect = _columnShapes[e.OldStartingIndex];

                _mainGrid!.Children.Remove(oldRect);
                _columnShapes.RemoveAt(e.OldStartingIndex);
            }

            return;
        }

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            var newDataList = e.NewItems!.Cast<double>().ToList();

            if (_currentColumnCount + newDataList.Count < MaximumColumns)
            {
                _currentColumnCount += newDataList.Count;
                _mainGrid!.ColumnDefinitions.Add(new ColumnDefinition());
            }
            else
            {
                for (var i = 0; i < newDataList.Count; i++)
                    lock (_locker)
                    {
                        var oldRect = _columnShapes[0];

                        _mainGrid!.Children.Remove(oldRect);
                        _columnShapes.RemoveAt(0);
                    }
            }

            foreach (var newRect in newDataList.Select(CreateNewRect))
                lock (_locker)
                {
                    _columnShapes.Add(newRect);
                    _mainGrid!.Children.Add(newRect);
                }

            lock (_locker)
            {
                foreach (var (i, rect) in _columnShapes.Index()) Grid.SetColumn(rect, i);
            }
        }
    }

    private Rectangle CreateNewRect(double value)
    {
        var rect = new Rectangle
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = value / Maximum * Bounds.Height
        };

        rect.Bind(Shape.FillProperty, this.GetObservable(DataColumnFillProperty));

        return rect;
    }

    private void OnDataSourceChanged()
    {
        if (!IsLoaded) return;

        using (_locker.EnterScope())
        {
            _mainGrid!.Children.Clear();
            _columnShapes.Clear();
        }

        if (DataSource == null) return;

        var newColDef = new ColumnDefinitions();
        for (var i = 0; i < DataSource.Count; i++)
            newColDef.Add(new ColumnDefinition());

        _mainGrid.ColumnDefinitions = newColDef;

        var startIndex = 0;

        foreach (var data in DataSource)
        {
            var rect = CreateNewRect(data);

            Grid.SetColumn(rect, startIndex++);

            using (_locker.EnterScope())
            {
                _columnShapes.Add(rect);
                _mainGrid.Children.Add(rect);
            }
        }

        DataSource.CollectionChanged += DataSourceOnCollectionChanged;
    }

    private void OnMaximumChanged()
    {
        if (DataSource == null) return;

        var end = Math.Min(_currentColumnCount, DataSource.Count);

        for (var i = 0; i < end; i++)
            using (_locker.EnterScope())
            {
                _columnShapes[i].Height = DataSource[i] / Maximum * Bounds.Height;
            }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == DataSourceProperty)
        {
            if (change.OldValue is ObservableCollection<double> old)
                old.CollectionChanged -= DataSourceOnCollectionChanged;

            OnDataSourceChanged();
        }

        if (change.Property == MaximumProperty)
            OnMaximumChanged();

        base.OnPropertyChanged(change);
    }
}