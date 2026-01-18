using System.Collections;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ThemeX.Controls.Scroll;

public sealed class AdvanceScrollContentPresenter : ScrollContentPresenter
{
    public static readonly StyledProperty<Vector> AnimatableOffsetProperty = AvaloniaProperty
        .Register<AdvanceScrollContentPresenter, Vector>(nameof(AnimatableOffset));

    private bool _isUpdatingByAnimation;
    private bool _isUpdatingByUser;
    private Vector _localOffset;

    static AdvanceScrollContentPresenter()
    {
        AnimatableOffsetProperty.Changed.AddClassHandler<AdvanceScrollContentPresenter>((presenter, args) =>
        {
            if (presenter._isUpdatingByUser) return;

            //presenter.WriteLine($"AnimatableOffset Changed with priority is {args.Priority}.");

            if (args is { Priority: BindingPriority.LocalValue, NewValue: Vector value })
                presenter._localOffset = value;

            var scrollViewer = presenter.FindAncestorOfType<ScrollViewer>();

            // Update offset of Presenter. The value will be also passed to ScrollViewer at the time.
            presenter._isUpdatingByAnimation = true;
            presenter.SetCurrentValue(OffsetProperty, presenter.AnimatableOffset);
            scrollViewer?.Offset = presenter._localOffset;
            presenter._isUpdatingByAnimation = false;
            //presenter.WriteLine($"Parent ScrollViewer.Offset is '{presenter.FindAncestorOfType<ScrollViewer>()?.Offset}'.");
        });

        ScrollViewer.OffsetProperty.Changed.AddClassHandler<SmoothScrollViewer>((viewer, args) =>
        {
            if (viewer.Presenter is not AdvanceScrollContentPresenter presenter) return;

            if (presenter._isUpdatingByAnimation) return;

            // Update AnimatableOffset when parent ScrollViewer's offset is changed.
            presenter._isUpdatingByUser = true;
            presenter._localOffset = viewer.Offset;
            presenter.AnimatableOffset = viewer.Offset;
            presenter._isUpdatingByUser = false;
        });
    }

    public AdvanceScrollContentPresenter()
    {
        // Remove the event handler for RequestBringIntoViewEvent that registered in the base ctor.
        TryRemoveRequestBringIntoViewEventHandler();

        // Set the Offset property to prevent the binding between it and the ScrollViewer's Offset.
        Offset = new Vector();
    }

    public Vector AnimatableOffset
    {
        get => GetValue(AnimatableOffsetProperty);
        private set => SetValue(AnimatableOffsetProperty, value);
    }

    private void TryRemoveRequestBringIntoViewEventHandler()
    {
        // TODO Check it when upgrade Avalonia.

        var eventHandlersFiled =
            typeof(Interactive).GetField("_eventHandlers", BindingFlags.NonPublic | BindingFlags.Instance);

        var dic = eventHandlersFiled?.GetValue(this) as IDictionary;
        dic?.Remove(RequestBringIntoViewEvent);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (TemplatedParent is not SmoothScrollViewer viewer)
        {
            base.OnPointerWheelChanged(e);
            return;
        }

        if (Extent.Height > Viewport.Height || Extent.Width > Viewport.Width)
        {
            var scrollable = Child as ILogicalScrollable;
            var isLogical = scrollable?.IsLogicalScrollEnabled == true;

            var x = Offset.X;
            var y = Offset.Y;

            if (Extent.Height > Viewport.Height)
            {
                var height = isLogical ? scrollable!.ScrollSize.Height : viewer.SmoothScrollingStep;
                y += -e.Delta.Y * height;
                y = Math.Max(y, 0);
                y = Math.Min(y, Extent.Height - Viewport.Height);
            }

            if (Extent.Width > Viewport.Width)
            {
                var width = isLogical ? scrollable!.ScrollSize.Width : viewer.SmoothScrollingStep;
                x += -e.Delta.X * width;
                x = Math.Max(x, 0);
                x = Math.Min(x, Extent.Width - Viewport.Width);
            }

            var newOffset = new Vector(x, y);
            var offsetChanged = newOffset != Offset;

            // Do not set it directly to keep template binding available.
            AnimatableOffset = newOffset;
            //viewer.Offset = newOffset;

            e.Handled = !IsScrollChainingEnabled || offsetChanged;
        }
    }
}