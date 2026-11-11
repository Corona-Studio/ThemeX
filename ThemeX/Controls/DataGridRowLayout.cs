using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ThemeX.Controls;

/// <summary>Places a row-state overlay behind the frozen grid without including it in track measurement.</summary>
public sealed class DataGridRowLayout : DataGridFrozenGrid
{
    public Control? Highlight
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            if (field != null)
            {
                VisualChildren.Remove(field);
                LogicalChildren.Remove(field);
            }
            field = value;
            if (value != null)
            {
                // Panel keeps Children at matching logical/visual indices. The overlay stays after them;
                // its negative ZIndex places it behind them without disturbing those indices.
                VisualChildren.Add(value);
                LogicalChildren.Add(value);
            }
            InvalidateMeasure();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = base.MeasureOverride(availableSize);
        Highlight?.Measure(availableSize);
        return desired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        Highlight?.Arrange(new Rect(arranged));
        return arranged;
    }
}
