using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;

namespace ThemeX.Helpers;

/// <summary>
///     Routes popups into the parent window's <see cref="OverlayLayer" /> when backdrop effects
///     are active and the host window is large enough, enabling live <see cref="Visual.BackdropEffect" />
///     blur behind menus, dropdowns, and flyouts without clipping small dialogs.
/// </summary>
public static partial class PopupOverlayHelper
{
    private const double MinHostWidth = 520;
    private const double MinHostHeight = 360;

    private static readonly AttachedProperty<TopLevel?> TrackedTopLevelProperty =
        AvaloniaProperty.RegisterAttached<Popup, TopLevel?>(
            "TrackedTopLevel",
            typeof(PopupOverlayHelper));

    private static readonly AttachedProperty<EventHandler<SizeChangedEventArgs>?> TopLevelSizeHandlerProperty =
        AvaloniaProperty.RegisterAttached<Popup, EventHandler<SizeChangedEventArgs>?>(
            "TopLevelSizeHandler",
            typeof(PopupOverlayHelper));

    static PopupOverlayHelper()
    {
        PreferOverlayForBackdropProperty.Changed.AddClassHandler(
            new Action<Popup, AvaloniaPropertyChangedEventArgs<bool>>(OnPopupPreferOverlayChanged));
        PreferOverlayForContextMenuProperty.Changed.AddClassHandler(
            new Action<ContextMenu, AvaloniaPropertyChangedEventArgs<bool>>(OnContextMenuPreferOverlayChanged));
        Popup.ChildProperty.Changed.AddClassHandler(
            new Action<Popup, AvaloniaPropertyChangedEventArgs<Control?>>(OnPopupChildChanged));
        Popup.PlacementTargetProperty.Changed.AddClassHandler(
            new Action<Popup, AvaloniaPropertyChangedEventArgs<Control?>>(OnPopupPlacementTargetChanged));
    }

    internal static void EnsureInitialized()
    {
        // Triggers the static constructor so ContextMenu popups are intercepted before their first Open().
    }

    [GeneratedAttachedProperty]
    public static partial bool GetPreferOverlayForBackdrop(Popup element);

    [GeneratedAttachedProperty]
    public static partial bool GetPreferOverlayForContextMenu(ContextMenu element);

    private static void OnPopupChildChanged(Popup popup, AvaloniaPropertyChangedEventArgs<Control?> args)
    {
        if (args.OldValue.Value is ContextMenu oldMenu)
            oldMenu.AttachedToLogicalTree -= OnContextMenuPopupAttachedToLogicalTree;

        if (args.NewValue.Value is ContextMenu menu)
        {
            menu.AttachedToLogicalTree -= OnContextMenuPopupAttachedToLogicalTree;
            menu.AttachedToLogicalTree += OnContextMenuPopupAttachedToLogicalTree;
            EnsureContextMenuOverlaySynced(menu);
        }
    }

    private static void OnContextMenuPopupAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        if (sender is ContextMenu menu)
            EnsureContextMenuOverlaySynced(menu);
    }

    private static void EnsureContextMenuOverlaySynced(ContextMenu menu)
    {
        if (menu.Parent is not Popup)
            return;

        if (!GetPreferOverlayForContextMenu(menu) && ((ILogical)menu).IsAttachedToLogicalTree)
            menu.ApplyStyling();

        if (GetPreferOverlayForContextMenu(menu))
            SyncContextMenuPopup(menu);
    }

    private static void OnPopupPreferOverlayChanged(Popup popup, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.OldValue.Value)
        {
            popup.AttachedToVisualTree -= OnPopupAttachedToVisualTree;
            popup.DetachedFromVisualTree -= OnPopupDetachedFromVisualTree;
            popup.ResourcesChanged -= OnPopupResourcesChanged;
            DetachTopLevelSizeTracking(popup);
        }

        if (args.NewValue.Value)
        {
            popup.AttachedToVisualTree += OnPopupAttachedToVisualTree;
            popup.DetachedFromVisualTree += OnPopupDetachedFromVisualTree;
            popup.ResourcesChanged += OnPopupResourcesChanged;
            AttachTopLevelSizeTracking(popup);
            UpdatePopupOverlay(popup);
        }
        else
        {
            popup.ClearValue(Popup.ShouldUseOverlayLayerProperty);
        }
    }

    private static void OnPopupPlacementTargetChanged(Popup popup, AvaloniaPropertyChangedEventArgs<Control?> args)
    {
        if (popup.Child is ContextMenu menu)
            EnsureContextMenuOverlaySynced(menu);

        if (GetPreferOverlayForBackdrop(popup))
        {
            AttachTopLevelSizeTracking(popup);
            UpdatePopupOverlay(popup, args.NewValue.Value);
        }
    }

    private static void OnPopupAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Popup popup && GetPreferOverlayForBackdrop(popup))
        {
            AttachTopLevelSizeTracking(popup);
            UpdatePopupOverlay(popup);
        }
    }

    private static void OnPopupDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Popup popup)
            DetachTopLevelSizeTracking(popup);
    }

    private static void OnPopupResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        if (sender is Popup popup && GetPreferOverlayForBackdrop(popup))
            UpdatePopupOverlay(popup);
    }

    private static void AttachTopLevelSizeTracking(Popup popup)
    {
        DetachTopLevelSizeTracking(popup);

        var target = popup.PlacementTarget ?? popup.Parent as Visual ?? popup;
        var topLevel = TopLevel.GetTopLevel(target);
        if (topLevel is null || topLevel is PopupRoot)
            return;

        EventHandler<SizeChangedEventArgs> handler = (_, _) =>
        {
            if (GetPreferOverlayForBackdrop(popup))
                UpdatePopupOverlay(popup);
        };

        topLevel.SizeChanged += handler;
        popup.SetValue(TrackedTopLevelProperty, topLevel);
        popup.SetValue(TopLevelSizeHandlerProperty, handler);
    }

    private static void DetachTopLevelSizeTracking(Popup popup)
    {
        if (popup.GetValue(TrackedTopLevelProperty) is { } topLevel &&
            popup.GetValue(TopLevelSizeHandlerProperty) is { } handler)
        {
            topLevel.SizeChanged -= handler;
        }

        popup.ClearValue(TrackedTopLevelProperty);
        popup.ClearValue(TopLevelSizeHandlerProperty);
    }

    private static void OnContextMenuPreferOverlayChanged(ContextMenu menu, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.OldValue.Value)
        {
            menu.Opening -= OnContextMenuOpening;
            menu.AttachedToLogicalTree -= OnContextMenuAttachedToLogicalTree;

            if (menu.Parent is Popup oldPopup)
                oldPopup.ClearValue(PreferOverlayForBackdropProperty);
        }

        if (args.NewValue.Value)
        {
            menu.Opening += OnContextMenuOpening;
            menu.AttachedToLogicalTree += OnContextMenuAttachedToLogicalTree;
            SyncContextMenuPopup(menu);
        }
    }

    private static void OnContextMenuAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        if (sender is ContextMenu menu && GetPreferOverlayForContextMenu(menu))
            SyncContextMenuPopup(menu);
    }

    private static void OnContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is ContextMenu menu && GetPreferOverlayForContextMenu(menu))
            SyncContextMenuPopup(menu);
    }

    private static void SyncContextMenuPopup(ContextMenu menu)
    {
        if (menu.Parent is not Popup popup)
            return;

        if (!GetPreferOverlayForBackdrop(popup))
            SetPreferOverlayForBackdrop(popup, true);

        UpdatePopupOverlay(popup, menu.PlacementTarget);
    }

    private static void UpdatePopupOverlay(Popup popup, Visual? anchor = null)
    {
        var backdropEnabled = IsBackdropEnabled(popup);
        if (!backdropEnabled)
        {
            popup.ShouldUseOverlayLayer = false;
            return;
        }

        var target = anchor ?? popup.PlacementTarget ?? popup.Parent as Visual ?? popup;
        var topLevel = TopLevel.GetTopLevel(target);
        if (topLevel is null || topLevel is PopupRoot || OverlayLayer.GetOverlayLayer(target) is null)
        {
            popup.ShouldUseOverlayLayer = false;
            return;
        }

        // Only route to OverlayLayer when the window has enough viewport room to contain flyouts comfortably.
        var size = topLevel.ClientSize.Width > 0 ? topLevel.ClientSize : topLevel.Bounds.Size;
        if (size.Width <= 0 && topLevel is Window window && !double.IsNaN(window.Width) && !double.IsNaN(window.Height))
            size = new Size(window.Width, window.Height);

        popup.ShouldUseOverlayLayer = size.Width >= MinHostWidth && size.Height >= MinHostHeight;
    }

    private static bool IsBackdropEnabled(StyledElement element)
    {
        if (element.TryFindResource(ThemeSettings.EnableBackdropEffectKey, out var value) && value is bool enabled)
            return enabled;

        return true;
    }
}
