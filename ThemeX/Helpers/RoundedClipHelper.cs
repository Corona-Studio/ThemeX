using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ThemeX.Helpers;

/// <summary>
///     Applies an exact rounded <see cref="Visual.Clip" /> matching a <see cref="Border" />'s
///     <see cref="Border.CornerRadius" /> so children and Avalonia 12.2 <see cref="Visual.BackdropEffect" />
///     blur are clipped to the rounded corner arc instead of the rectangular bounds.
/// </summary>
public static partial class RoundedClipHelper
{
    static RoundedClipHelper()
    {
        ClipToCornerRadiusProperty.Changed.AddClassHandler(
            new Action<Border, AvaloniaPropertyChangedEventArgs<bool>>(OnClipToCornerRadiusChanged));
    }

    [GeneratedAttachedProperty]
    public static partial bool GetClipToCornerRadius(Border element);

    private static void OnClipToCornerRadiusChanged(Border border, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.OldValue.Value)
        {
            border.PropertyChanged -= OnBorderPropertyChanged;
            border.AttachedToVisualTree -= OnBorderAttachedToVisualTree;
            border.ClearValue(Visual.ClipProperty);
        }

        if (args.NewValue.Value)
        {
            border.PropertyChanged += OnBorderPropertyChanged;
            border.AttachedToVisualTree += OnBorderAttachedToVisualTree;
            UpdateClip(border);
        }
    }

    private static void OnBorderAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Border border)
            UpdateClip(border);
    }

    private static void OnBorderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (sender is not Border border)
            return;

        // Clip geometry uses local size, independent of the border's position.
        // Skip position-only Bounds updates (e.g. scrolling or container moves).
        if (change.Property == Visual.BoundsProperty)
        {
            if (change.GetOldValue<Rect>().Size != change.GetNewValue<Rect>().Size)
                UpdateClip(border);
        }
        else if (change.Property == Border.CornerRadiusProperty)
        {
            if (change.GetOldValue<CornerRadius>() != change.GetNewValue<CornerRadius>())
                UpdateClip(border);
        }
    }

    private static void UpdateClip(Border border)
    {
        var size = border.Bounds.Size;
        var cornerRadius = border.CornerRadius;

        if (border.Clip is RectangleGeometry existingRect &&
            size.Width > 0 &&
            size.Height > 0 &&
            cornerRadius.TopLeft == cornerRadius.TopRight &&
            cornerRadius.TopRight == cornerRadius.BottomRight &&
            cornerRadius.BottomRight == cornerRadius.BottomLeft &&
            double.IsFinite(cornerRadius.TopLeft) &&
            cornerRadius.TopLeft > 0)
        {
            var clampedRadius = Math.Min(cornerRadius.TopLeft, Math.Min(size.Width, size.Height) / 2.0);
            if (existingRect.Rect == new Rect(0, 0, size.Width, size.Height) &&
                existingRect.RadiusX == clampedRadius &&
                existingRect.RadiusY == clampedRadius)
            {
                return;
            }
        }

        var clip = GeometryHelper.CreateRoundedClipGeometry(size, cornerRadius);
        if (clip is null && border.Clip is null)
            return;

        border.Clip = clip;
    }
}
