using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace ThemeX.Helpers;

/// <summary>
/// Defers a popup's close long enough for its child presenter to fade out.
/// </summary>
public static class PopupFadeAnimationHelper
{
    public static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(120);

    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Popup, bool>(
            "IsEnabled",
            typeof(PopupFadeAnimationHelper));

    private static readonly AttachedProperty<AnimationState?> StateProperty =
        AvaloniaProperty.RegisterAttached<Popup, AnimationState?>(
            "State",
            typeof(PopupFadeAnimationHelper));

    static PopupFadeAnimationHelper()
    {
        IsEnabledProperty.Changed.AddClassHandler(
            new Action<Popup, AvaloniaPropertyChangedEventArgs<bool>>(OnIsEnabledChanged));
        Popup.IsOpenProperty.Changed.AddClassHandler(
            new Action<Popup, AvaloniaPropertyChangedEventArgs<bool>>(OnIsOpenChanged));
    }

    public static bool GetIsEnabled(Popup element) => element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Popup element, bool value) => element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(Popup popup, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.OldValue.Value)
            Detach(popup);

        if (args.NewValue.Value)
            Attach(popup);
    }

    private static void Attach(Popup popup)
    {
        popup.SetValue(StateProperty, new AnimationState());
        popup.Opened += OnOpened;
        popup.Closed += OnClosed;
        AddClosingHandler(popup, OnClosing);
    }

    private static void Detach(Popup popup)
    {
        popup.Opened -= OnOpened;
        popup.Closed -= OnClosed;
        RemoveClosingHandler(popup, OnClosing);

        if (popup.GetValue(StateProperty) is { } state)
            state.CancelPendingClose();

        popup.ClearValue(StateProperty);
        ResetPresenter(popup);
    }

    private static void OnOpened(object? sender, EventArgs e)
    {
        if (sender is not Popup popup || popup.GetValue(StateProperty) is not { } state)
            return;

        state.CancelPendingClose();
        state.IsClosing = false;
        state.AllowNextClose = false;

        if (popup.Child is not { } presenter)
            return;

        presenter.IsHitTestVisible = true;
        presenter.Opacity = 0;
        Dispatcher.UIThread.Post(() =>
        {
            if (popup.IsOpen && !state.IsClosing && ReferenceEquals(popup.GetValue(StateProperty), state))
                presenter.Opacity = 1;
        }, DispatcherPriority.Render);
    }

    private static void OnClosing(object? sender, CancelEventArgs e)
    {
        if (sender is not Popup popup || popup.GetValue(StateProperty) is not { } state)
            return;

        if (state.AllowNextClose || !popup.IsAttachedToVisualTree())
        {
            state.AllowNextClose = false;
            return;
        }

        e.Cancel = true;
        if (state.IsClosing)
            return;

        state.IsClosing = true;

        // A menu-bar popup is a separate root from any nested submenus. Update
        // the logical state now so all levels start fading at the same time.
        if (popup.TemplatedParent is MenuItem { IsSubMenuOpen: true } menuItem)
            menuItem.IsSubMenuOpen = false;

        if (popup.Child is { } presenter)
        {
            presenter.IsHitTestVisible = false;
            presenter.Opacity = 0;
        }

        var token = state.BeginPendingClose();
        _ = FinishCloseAsync(popup, state, token);
    }

    private static async Task FinishCloseAsync(
        Popup popup,
        AnimationState state,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(FadeDuration, cancellationToken).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (cancellationToken.IsCancellationRequested ||
                    !state.IsClosing ||
                    !ReferenceEquals(popup.GetValue(StateProperty), state))
                    return;

                state.AllowNextClose = true;
                popup.Close();
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Reopening the popup cancels its pending close.
        }
    }

    private static void OnClosed(object? sender, EventArgs e)
    {
        if (sender is not Popup popup || popup.GetValue(StateProperty) is not { } state)
            return;

        state.CancelPendingClose();
        state.IsClosing = false;
        state.AllowNextClose = false;
        ResetPresenter(popup, opacity: 0);
    }

    private static void OnIsOpenChanged(Popup popup, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (!args.NewValue.Value || !GetIsEnabled(popup) ||
            popup.GetValue(StateProperty) is not { IsClosing: true } state)
            return;

        // The close may have originated from a two-way binding. If the owner
        // reopens while the fade is running, keep the existing popup host and
        // smoothly restore it instead of flashing a second popup.
        state.CancelPendingClose();
        state.IsClosing = false;
        state.AllowNextClose = false;
        ResetPresenter(popup);
    }

    private static void ResetPresenter(Popup popup, double opacity = 1)
    {
        if (popup.Child is not { } presenter)
            return;

        presenter.IsHitTestVisible = true;
        presenter.Opacity = opacity;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "add_Closing")]
    private static extern void AddClosingHandler(Popup popup, EventHandler<CancelEventArgs> handler);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "remove_Closing")]
    private static extern void RemoveClosingHandler(Popup popup, EventHandler<CancelEventArgs> handler);

    private sealed class AnimationState
    {
        private CancellationTokenSource? _closeCancellation;

        public bool IsClosing { get; set; }
        public bool AllowNextClose { get; set; }

        public CancellationToken BeginPendingClose()
        {
            CancelPendingClose();
            _closeCancellation = new CancellationTokenSource();
            return _closeCancellation.Token;
        }

        public void CancelPendingClose()
        {
            if (_closeCancellation is not { } cancellation)
                return;

            _closeCancellation = null;
            cancellation.Cancel();
            cancellation.Dispose();
        }
    }
}
