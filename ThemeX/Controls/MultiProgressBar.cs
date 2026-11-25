using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Diagnostics;
using ThemeX.Helpers;

namespace ThemeX.Controls;

public record Indicator(string Text, ImmutableSolidColorBrush Color);

public enum IndicatorColorGradientMode
{
    Lighten,
    Darken
}

[TemplatePart("Indicator", typeof(ItemsControl))]
[TemplatePart("MainGrid", typeof(Grid), IsRequired = true)]
public sealed partial class MultiProgressBar : TemplatedControl
{
    public static readonly StyledProperty<ISolidColorBrush> FillColorProperty =
        AvaloniaProperty.Register<MultiProgressBar, ISolidColorBrush>(
            nameof(FillColor),
            new ImmutableSolidColorBrush(Colors.BurlyWood));

    private ItemsControl? _indicators;
    private Grid? _mainGrid;

    [GeneratedDirectProperty]
    public partial List<double>? Values { get; set; }

    [GeneratedDirectProperty]
    public partial List<string>? Tags { get; set; }

    [GeneratedStyledProperty(DefaultValue = "####")]
    public partial string ValueFormat { get; set; }

    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool IndicatorVisibility { get; set; }

    public ISolidColorBrush FillColor
    {
        get => GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    [GeneratedStyledProperty(DefaultValue = IndicatorColorGradientMode.Lighten)]
    public partial IndicatorColorGradientMode ColorGradientMode { get; set; }

    public ObservableCollection<Indicator> Items { get; } = [];

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _indicators = e.NameScope.Find<ItemsControl>("Indicators")!;
        _mainGrid = e.NameScope.Find<Grid>("MainGrid")!;

        _indicators.ItemsSource = Items;

        OnDataPropertyChanged();
        OnIndicatorVisibilityChanged();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IndicatorVisibilityProperty)
            OnIndicatorVisibilityChanged();
        else if (change.Property == FillColorProperty)
            OnFillColorChanged();
        else if (change.Property == ValuesProperty ||
                 change.Property == TagsProperty ||
                 change.Property == ValueFormatProperty ||
                 change.Property == ColorGradientModeProperty)
            OnDataPropertyChanged();
    }

    private void OnDataPropertyChanged()
    {
        if (_mainGrid == null) return;
        if (Values == null || Values.Count == 0) return;
        if (Tags == null || Tags.Count == 0) return;

        _mainGrid.Children.Clear();
        _mainGrid.ColumnDefinitions.Clear();
        Items.Clear();

        foreach (var val in Values)
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(val, GridUnitType.Star)));

        for (var i = 0; i < Values.Count; i++)
        {
            var targetColor = FillColor.Color;

            for (var j = 0; j < i; j++)
                targetColor = GetNextIndicatorColor(targetColor, ColorGradientMode);

            var colorBrush = new ImmutableSolidColorBrush(targetColor);
            var rect = new Rectangle { Fill = colorBrush };

            Grid.SetColumn(rect, i);
            _mainGrid.Children.Add(rect);

            if (i >= 0 && i < Tags.Count)
                Items.Add(new Indicator($"{Tags[i]} [{Values[i].ToString(ValueFormat)}]",
                    colorBrush));
        }
    }

    private void OnIndicatorVisibilityChanged()
    {
        _indicators?.IsVisible = IndicatorVisibility;
    }

    private void OnFillColorChanged()
    {
        if (_mainGrid == null) return;
        if (_indicators == null) return;

        var index = 0;

        foreach (var control in _mainGrid.Children)
        {
            if (control is not Rectangle rect) continue;

            var targetColor = FillColor.Color;

            for (var j = 0; j < index; j++)
                targetColor = GetNextIndicatorColor(targetColor, ColorGradientMode);

            var colorBrush = new ImmutableSolidColorBrush(targetColor);

            rect.Fill = colorBrush;

            if (_indicators.ItemCount > index)
            {
                var oldIndicator = Items[index];
                var newIndicator = oldIndicator with { Color = colorBrush };

                Items[index] = newIndicator;
            }

            index++;
        }
    }

    private static Color GetNextIndicatorColor(Color color, IndicatorColorGradientMode mode)
    {
        if (!Enum.IsDefined(mode))
            return ThrowHelper.ThrowArgumentOutOfRangeException<Color>(nameof(ColorGradientMode));

        // Unknown numeric values are rejected above. A new named mode must add a branch.
#pragma warning disable CS8524
        return mode switch
        {
            IndicatorColorGradientMode.Lighten => color.ToLighter(),
            IndicatorColorGradientMode.Darken => color.ToDarker()
        };
#pragma warning restore CS8524
    }
}
