using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Diagnostics;
using LauncherX.SourceGenerator.Shared;
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
    private static readonly SolidColorBrush DefaultFillColor = new(Colors.BurlyWood);

    private ItemsControl? _indicators;
    private Grid? _mainGrid;

    [StyledProp] public partial List<double>? Values { get; set; }

    [StyledProp] public partial List<string>? Tags { get; set; }

    [StyledProp(DefaultValue = "####")] public partial string ValueFormat { get; set; }

    [StyledProp(DefaultValue = true)] public partial bool IndicatorVisibility { get; set; }

    [StyledProp(DefaultValue = nameof(DefaultFillColor))]
    public partial ISolidColorBrush FillColor { get; set; }

    [StyledProp(DefaultValue = IndicatorColorGradientMode.Lighten)]
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
                targetColor = ColorGradientMode switch
                {
                    IndicatorColorGradientMode.Lighten => targetColor.ToLighter(),
                    IndicatorColorGradientMode.Darken => targetColor.ToDarker(),
                    _ => ThrowHelper.ThrowArgumentOutOfRangeException<Color>(nameof(ColorGradientMode))
                };

            var colorBrush = new ImmutableSolidColorBrush(targetColor);
            var rect = new Rectangle { Fill = colorBrush };

            Grid.SetColumn(rect, i);
            _mainGrid.Children.Add(rect);

            if (i >= 0 && i < Tags.Count)
                Items.Add(new Indicator($"{Tags![i]} [{Values![i].ToString(ValueFormat)}]",
                    colorBrush));
        }
    }

    partial void OnIndicatorVisibilityChanged()
    {
        _indicators?.IsVisible = IndicatorVisibility;
    }

    partial void OnFillColorChanged()
    {
        if (_mainGrid == null) return;
        if (_indicators == null) return;

        var index = 0;

        foreach (var control in _mainGrid.Children)
        {
            if (control is not Rectangle rect) continue;

            var targetColor = FillColor.Color;

            for (var j = 0; j < index; j++)
                targetColor = ColorGradientMode switch
                {
                    IndicatorColorGradientMode.Lighten => targetColor.ToLighter(),
                    IndicatorColorGradientMode.Darken => targetColor.ToDarker(),
                    _ => ThrowHelper.ThrowArgumentOutOfRangeException<Color>(nameof(ColorGradientMode))
                };

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

    partial void OnValuesChanged()
    {
        OnDataPropertyChanged();
    }

    partial void OnTagsChanged()
    {
        OnDataPropertyChanged();
    }

    partial void OnValueFormatChanged()
    {
        OnDataPropertyChanged();
    }
}