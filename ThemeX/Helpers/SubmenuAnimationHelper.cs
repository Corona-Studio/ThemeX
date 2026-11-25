using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace ThemeX.Helpers;

/// <summary>
/// Keeps a menu item's popup alive briefly after IsSubMenuOpen becomes false,
/// allowing the popup presenter to finish its opacity transition.
/// </summary>
public static partial class SubmenuAnimationHelper
{
    public static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(120);

    private static readonly AttachedProperty<AnimationState?> StateProperty =
        AvaloniaProperty.RegisterAttached<MenuItem, AnimationState?>(
            "State",
            typeof(SubmenuAnimationHelper));

    static SubmenuAnimationHelper()
    {
        IsEnabledProperty.Changed.AddClassHandler(
            new Action<MenuItem, AvaloniaPropertyChangedEventArgs<bool>>(OnIsEnabledChanged));
        MenuItem.IsSubMenuOpenProperty.Changed.AddClassHandler(
            new Action<MenuItem, AvaloniaPropertyChangedEventArgs<bool>>(OnIsSubMenuOpenChanged));
    }

    [GeneratedAttachedProperty]
    public static partial bool GetIsEnabled(MenuItem element);

    [GeneratedAttachedProperty]
    public static partial bool GetIsPopupOpen(MenuItem element);

    [GeneratedAttachedProperty]
    public static partial double GetOpacity(MenuItem element);

    private static void OnIsEnabledChanged(MenuItem menuItem, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.OldValue.Value && menuItem.GetValue(StateProperty) is { } oldState)
        {
            oldState.CancelPendingClose();
            menuItem.ClearValue(StateProperty);
        }

        if (!args.NewValue.Value)
            return;

        menuItem.SetValue(StateProperty, new AnimationState());
        SetIsPopupOpen(menuItem, menuItem.IsSubMenuOpen);
        SetOpacity(menuItem, menuItem.IsSubMenuOpen ? 1 : 0);
    }

    private static void OnIsSubMenuOpenChanged(
        MenuItem menuItem,
        AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (!GetIsEnabled(menuItem) || menuItem.GetValue(StateProperty) is not { } state)
            return;

        if (args.NewValue.Value)
            BeginOpen(menuItem, state);
        else
            BeginClose(menuItem, state);
    }

    private static void BeginOpen(MenuItem menuItem, AnimationState state)
    {
        state.CancelPendingClose();
        SetOpacity(menuItem, 0);
        SetIsPopupOpen(menuItem, true);

        Dispatcher.UIThread.Post(() =>
        {
            if (menuItem.IsSubMenuOpen && ReferenceEquals(menuItem.GetValue(StateProperty), state))
                SetOpacity(menuItem, 1);
        }, DispatcherPriority.Render);
    }

    private static void BeginClose(MenuItem menuItem, AnimationState state)
    {
        if (!GetIsPopupOpen(menuItem))
            return;

        SetOpacity(menuItem, 0);
        var token = state.BeginPendingClose();
        _ = FinishCloseAsync(menuItem, state, token);
    }

    private static async Task FinishCloseAsync(
        MenuItem menuItem,
        AnimationState state,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(FadeDuration, cancellationToken).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (cancellationToken.IsCancellationRequested ||
                    menuItem.IsSubMenuOpen ||
                    !ReferenceEquals(menuItem.GetValue(StateProperty), state))
                    return;

                SetIsPopupOpen(menuItem, false);
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Reopening a submenu cancels its pending close without flashing.
        }
    }

    private sealed class AnimationState
    {
        private CancellationTokenSource? _closeCancellation;

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
