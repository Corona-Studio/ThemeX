using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace ThemeX.Helpers;

/// <summary>Reveals scrollbars on scrolling or pointer proximity, then contracts before fading.</summary>
public static partial class ScrollBarAutoHideHelper
{
    private static readonly AttachedProperty<State?> StateProperty =
        AvaloniaProperty.RegisterAttached<ScrollBar, State?>("State", typeof(ScrollBarAutoHideHelper));

    static ScrollBarAutoHideHelper()
    {
        IsEnabledProperty.Changed.AddClassHandler<ScrollBar>((bar, args) =>
        {
            bar.GetValue(StateProperty)?.Dispose();
            bar.ClearValue(StateProperty);
            if (args.NewValue is true)
                bar.SetValue(StateProperty, new State(bar));
        });
    }

    [GeneratedAttachedProperty]
    public static partial bool GetIsEnabled(ScrollBar bar);

    private sealed class State : IDisposable
    {
        private readonly ScrollBar _bar;
        private readonly DispatcherTimer _timer = new();
        private Control? _owner;
        private Window? _window;
        private bool _near;
        private bool _dragging;
        private bool _contracted;

        public State(ScrollBar bar)
        {
            _bar = bar;
            _timer.Tick += OnTick;
            bar.AttachedToVisualTree += OnAttached;
            bar.DetachedFromVisualTree += OnDetached;
            bar.PropertyChanged += OnPropertyChanged;
            bar.AddHandler(Thumb.DragStartedEvent, OnDragStarted, RoutingStrategies.Bubble, true);
            bar.AddHandler(Thumb.DragCompletedEvent, OnDragCompleted, RoutingStrategies.Bubble, true);
            if (bar.IsAttachedToVisualTree()) Attach();
        }

        private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e) => Attach();
        private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) => Detach();

        private void Attach()
        {
            Detach();
            _owner = _bar.TemplatedParent as Control ?? _bar;
            _owner.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, true);
            _owner.PointerExited += OnPointerExited;
            _window = TopLevel.GetTopLevel(_bar) as Window;
            _window?.Deactivated += OnDeactivated;
        }

        private void Detach()
        {
            _window?.Deactivated -= OnDeactivated;
            _window = null;
            if (_owner is { } owner)
            {
                owner.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
                owner.PointerExited -= OnPointerExited;
                _owner = null;
            }
            _timer.Stop();
            _near = _dragging = _contracted = false;
            _bar.Classes.Remove("auto-hide-visible");
            _bar.Classes.Remove("auto-hide-expanded");
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_bar.AllowAutoHide || !_bar.IsVisible || _owner is null) return;
            var origin = _bar.TranslatePoint(default, _owner);
            if (origin is null) return;
            var near = new Rect(origin.Value, _bar.Bounds.Size).Inflate(12).Contains(e.GetPosition(_owner));
            if (near == _near) return;
            _near = near;
            if (near) Reveal();
            else Contract();
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            _near = false;
            Contract();
        }

        private void OnDeactivated(object? sender, EventArgs e)
        {
            _near = false;
            Contract();
        }

        private void OnDragStarted(object? sender, VectorEventArgs e)
        {
            _dragging = true;
            Reveal();
        }

        private void OnDragCompleted(object? sender, VectorEventArgs e)
        {
            _dragging = false;
            if (!_near) Contract();
        }

        private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ScrollBar.ValueProperty && _owner is not null && _bar.IsVisible)
                Reveal();
            else if (e.Property == ScrollBar.AllowAutoHideProperty)
            {
                _timer.Stop();
                _bar.Classes.Remove("auto-hide-visible");
                _bar.Classes.Remove("auto-hide-expanded");
                if (_bar.AllowAutoHide && (_near || _dragging)) Reveal();
            }
        }

        private void Reveal()
        {
            if (!_bar.AllowAutoHide) return;
            _timer.Stop();
            _contracted = false;
            _bar.Classes.Add("auto-hide-visible");
            _bar.Classes.Add("auto-hide-expanded");
            if (!_near && !_dragging) Schedule(TimeSpan.FromMilliseconds(700));
        }

        private void Contract()
        {
            if (_dragging || !_bar.AllowAutoHide) return;
            _timer.Stop();
            _bar.Classes.Remove("auto-hide-expanded");
            _contracted = true;
            if (_bar.Classes.Contains("auto-hide-visible")) Schedule(TimeSpan.FromMilliseconds(250));
        }

        private void Schedule(TimeSpan delay)
        {
            _timer.Interval = delay;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _timer.Stop();
            if (_near || _dragging) return;
            if (!_contracted) Contract();
            else _bar.Classes.Remove("auto-hide-visible");
        }

        public void Dispose()
        {
            Detach();
            _bar.AttachedToVisualTree -= OnAttached;
            _bar.DetachedFromVisualTree -= OnDetached;
            _bar.PropertyChanged -= OnPropertyChanged;
            _bar.RemoveHandler(Thumb.DragStartedEvent, OnDragStarted);
            _bar.RemoveHandler(Thumb.DragCompletedEvent, OnDragCompleted);
            _timer.Tick -= OnTick;
        }
    }
}
