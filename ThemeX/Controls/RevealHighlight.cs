using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using ThemeX.Controls;

namespace ThemeX;

/// <summary>
///     Selects which parts of Fluent Reveal Highlight are rendered.
/// </summary>
public enum RevealHighlightMode
{
    BorderAndHover,
    BorderOnly,
    HoverOnly,
    None
}

/// <summary>
///     Selects the visual palette for Fluent Reveal Highlight.
/// </summary>
public enum RevealHighlightTone
{
    /// <summary>Standard control/card surface (luminous white in Dark, crisp edge + white sheen in Light).</summary>
    Standard,

    /// <summary>Accent-colored surface such as PrimaryButton or checked ToggleButton (gleaming white).</summary>
    Accent,

    /// <summary>Lower-intensity glow for clickable hero cards and subtle surfaces.</summary>
    Subtle
}

/// <summary>
///     Attached properties that opt any Avalonia <see cref="Control" /> into ThemeX's window-wide
///     Fluent Reveal Highlight (mouse-tracking proximity border light + hover spotlight), or customize
///     / disable Reveal on ThemeX controls that enable it by default.
/// </summary>
public static partial class RevealHighlight
{
    private static readonly AttachedProperty<AdornerAttachment?> AdornerAttachmentProperty =
        AvaloniaProperty.RegisterAttached<Control, AdornerAttachment?>(
            "AdornerAttachment",
            typeof(RevealHighlight));

    private static readonly AttachedProperty<RevealOverlay?> TemplateOverlayProperty =
        AvaloniaProperty.RegisterAttached<Control, RevealOverlay?>(
            "TemplateOverlay",
            typeof(RevealHighlight));

    private static readonly AttachedProperty<ScopeAttachment?> ScopeAttachmentProperty =
        AvaloniaProperty.RegisterAttached<Control, ScopeAttachment?>(
            "ScopeAttachment",
            typeof(RevealHighlight));

    static RevealHighlight()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
        IsScopeProperty.Changed.AddClassHandler<Control>(OnIsScopeChanged);
        ModeProperty.Changed.AddClassHandler<Control>(OnVisualOptionChanged);
        ToneProperty.Changed.AddClassHandler<Control>(OnVisualOptionChanged);
        BorderLightRadiusProperty.Changed.AddClassHandler<Control>(OnVisualOptionChanged);
        HoverLightRadiusProperty.Changed.AddClassHandler<Control>(OnVisualOptionChanged);
        BorderThicknessProperty.Changed.AddClassHandler<Control>(OnVisualOptionChanged);
        CornerRadiusProperty.Changed.AddClassHandler<Control>(OnVisualOptionChanged);
    }

    internal static void EnsureInitialized()
    {
        // Triggers static constructor to register class handlers before XAML parsing.
    }

    [GeneratedAttachedProperty(DefaultValue = false)]
    public static partial bool GetIsEnabled(Control element);

    /// <summary>
    ///     Marks an overlay, drawer, modal scrim, or notification host as an isolated Reveal Highlight scope.
    ///     Pointer movement over an active scope never illuminates controls outside that scope, and toggling
    ///     the scope's visibility or hit-testability immediately extinguishes stale highlights.
    /// </summary>
    [GeneratedAttachedProperty(DefaultValue = false)]
    public static partial bool GetIsScope(Control element);

    [GeneratedAttachedProperty(DefaultValue = RevealHighlightMode.BorderAndHover)]
    public static partial RevealHighlightMode GetMode(Control element);

    [GeneratedAttachedProperty(DefaultValue = RevealHighlightTone.Standard)]
    public static partial RevealHighlightTone GetTone(Control element);

    [GeneratedAttachedProperty(DefaultValue = 84d)]
    public static partial double GetBorderLightRadius(Control element);

    [GeneratedAttachedProperty(DefaultValue = 112d)]
    public static partial double GetHoverLightRadius(Control element);

    [GeneratedAttachedProperty(DefaultValue = 1.0d)]
    public static partial double GetBorderThickness(Control element);

    [GeneratedAttachedProperty]
    public static partial CornerRadius? GetCornerRadius(Control element);

    internal static void RegisterTemplateOverlay(Control host, RevealOverlay overlay)
    {
        // If an adorner was attached before the control's template applied, tear it down cleanly
        // so the built-in template overlay is the single renderer.
        if (host.GetValue(AdornerAttachmentProperty) is { } existingAdorner)
        {
            existingAdorner.Dispose();
            host.ClearValue(AdornerAttachmentProperty);
        }

        host.SetValue(TemplateOverlayProperty, overlay);
    }

    internal static void UnregisterTemplateOverlay(Control host, RevealOverlay overlay)
    {
        if (ReferenceEquals(host.GetValue(TemplateOverlayProperty), overlay))
            host.ClearValue(TemplateOverlayProperty);
    }

    private static void OnIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (control.GetValue(TemplateOverlayProperty) is { } templateOverlay)
        {
            templateOverlay.NotifyHostOptionsChanged();
            return;
        }

        if (args.NewValue is true)
        {
            if (control.GetValue(AdornerAttachmentProperty) is null)
                control.SetValue(AdornerAttachmentProperty, new AdornerAttachment(control));
        }
        else if (control.GetValue(AdornerAttachmentProperty) is { } attachment)
        {
            attachment.Dispose();
            control.ClearValue(AdornerAttachmentProperty);
        }
    }

    private static void OnIsScopeChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is true)
        {
            if (control.GetValue(ScopeAttachmentProperty) is null)
                control.SetValue(ScopeAttachmentProperty, new ScopeAttachment(control));
        }
        else if (control.GetValue(ScopeAttachmentProperty) is { } attachment)
        {
            attachment.Dispose();
            control.ClearValue(ScopeAttachmentProperty);
        }
    }

    private static void OnVisualOptionChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (control.GetValue(TemplateOverlayProperty) is { } templateOverlay)
        {
            templateOverlay.NotifyHostOptionsChanged();
            return;
        }

        control.GetValue(AdornerAttachmentProperty)?.SyncOverlay();
    }

    private sealed class ScopeAttachment : IDisposable
    {
        private readonly Control _control;
        private TopLevel? _topLevel;
        private bool _disposed;

        public ScopeAttachment(Control control)
        {
            _control = control;
            _control.AttachedToVisualTree += OnAttachedToVisualTree;
            _control.DetachedFromVisualTree += OnDetachedFromVisualTree;
            _control.PropertyChanged += OnPropertyChanged;

            if (_control.IsAttachedToVisualTree())
            {
                _topLevel = TopLevel.GetTopLevel(_control);
                NotifyScopeChanged();
            }
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_disposed)
                return;

            _topLevel = TopLevel.GetTopLevel(_control) ?? e.RootVisual as TopLevel;
            if (_control.IsVisible && _control.IsHitTestVisible)
                NotifyScopeChanged();
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            var topLevel = _topLevel ?? e.RootVisual as TopLevel;
            _topLevel = null;
            if (topLevel is not null)
                RevealOverlay.NotifyScopeChanged(topLevel);
        }

        private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_disposed)
                return;

            if (e.Property == Visual.IsVisibleProperty ||
                e.Property == InputElement.IsHitTestVisibleProperty)
            {
                NotifyScopeChanged();
            }
        }

        private void NotifyScopeChanged()
        {
            var topLevel = _topLevel ?? TopLevel.GetTopLevel(_control);
            if (topLevel is not null)
            {
                _topLevel = topLevel;
                RevealOverlay.NotifyScopeChanged(topLevel);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _control.AttachedToVisualTree -= OnAttachedToVisualTree;
            _control.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            _control.PropertyChanged -= OnPropertyChanged;

            var topLevel = _topLevel ?? TopLevel.GetTopLevel(_control);
            _topLevel = null;
            if (topLevel is not null)
                RevealOverlay.NotifyScopeChanged(topLevel);
        }
    }

    private sealed class AdornerAttachment : IDisposable
    {
        private readonly Control _target;
        private AdornerLayer? _layer;
        private RevealOverlay? _overlay;
        private bool _disposed;

        public AdornerAttachment(Control target)
        {
            _target = target;
            _target.AttachedToVisualTree += OnAttachedToVisualTree;
            _target.DetachedFromVisualTree += OnDetachedFromVisualTree;

            if (_target.IsAttachedToVisualTree())
                Attach();
        }

        public void SyncOverlay()
        {
            _overlay?.NotifyHostOptionsChanged();
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (!_disposed)
                Attach();
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Detach();
        }

        private void Attach()
        {
            if (_overlay is not null || _target.GetValue(TemplateOverlayProperty) is not null)
                return;

            var layer = AdornerLayer.GetAdornerLayer(_target);
            if (layer is null)
                return;

            var overlay = RevealOverlay.CreateForAdornedTarget(_target);
            AdornerLayer.SetAdornedElement(overlay, _target);
            AdornerLayer.SetIsClipEnabled(overlay, true);

            layer.Children.Add(overlay);
            _layer = layer;
            _overlay = overlay;
        }

        private void Detach()
        {
            if (_overlay is not null && _layer is not null)
            {
                _layer.Children.Remove(_overlay);
                AdornerLayer.SetAdornedElement(_overlay, null);
            }

            _overlay = null;
            _layer = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _target.AttachedToVisualTree -= OnAttachedToVisualTree;
            _target.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            Detach();
        }
    }
}
