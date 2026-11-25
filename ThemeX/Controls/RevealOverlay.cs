using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ThemeX.Helpers;

namespace ThemeX.Controls;

/// <summary>
///     Zero-hit-test GPU-rendered overlay that draws Fluent Design Reveal Highlight:
///     window-wide proximity border illumination + surface hover spotlight clipped to the control's CornerRadius.
/// </summary>
public partial class RevealOverlay : Control
{
    private Control? _adornedTarget;
    private Control? _templateHost;
    private TopLevel? _trackedTopLevel;
    private Point? _localPointer;
    private bool _wasLit;
    private bool _paletteValid;
    private ThemeVariant? _cachedThemeVariant;
    private RevealPalette _cachedPalette;

    public RevealOverlay()
    {
        IsHitTestVisible = false;
        Focusable = false;
        ClipToBounds = false;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        ResourcesChanged += OnResourcesChanged;
    }

    [GeneratedStyledProperty(DefaultValue = true)]
    public partial bool IsRevealEnabled { get; set; }

    [GeneratedStyledProperty(DefaultValue = RevealHighlightMode.BorderAndHover)]
    public partial RevealHighlightMode Mode { get; set; }

    [GeneratedStyledProperty(DefaultValue = RevealHighlightTone.Standard)]
    public partial RevealHighlightTone Tone { get; set; }

    [GeneratedStyledProperty]
    public partial CornerRadius CornerRadius { get; set; }

    [GeneratedStyledProperty(DefaultValue = 1.0d)]
    public partial double RevealBorderThickness { get; set; }

    [GeneratedStyledProperty(DefaultValue = 84d)]
    public partial double BorderLightRadius { get; set; }

    [GeneratedStyledProperty(DefaultValue = 112d)]
    public partial double HoverLightRadius { get; set; }

    [GeneratedStyledProperty(DefaultValue = false)]
    public partial bool IsPressedState { get; set; }

    internal static RevealOverlay CreateForAdornedTarget(Control target)
    {
        return new RevealOverlay
        {
            _adornedTarget = target,
            IsRevealEnabled = true
        };
    }

    internal void NotifyHostOptionsChanged()
    {
        if (!IsEffectivelyActive())
            ClearLight();
        else if (_wasLit)
            InvalidateVisual();
    }

    internal static void NotifyScopeChanged(TopLevel topLevel)
    {
        RevealTracker.TryGet(topLevel)?.InvalidateScope();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _paletteValid = false;

        if (_adornedTarget is null && TemplatedParent is Control host)
        {
            _templateHost = host;
            RevealHighlight.RegisterTemplateOverlay(host, this);
        }

        _trackedTopLevel = TopLevel.GetTopLevel(this);
        if (_trackedTopLevel is not null)
            RevealTracker.GetOrCreate(_trackedTopLevel).Register(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_trackedTopLevel is not null)
        {
            RevealTracker.TryGet(_trackedTopLevel)?.Unregister(this);
            _trackedTopLevel = null;
        }

        if (_templateHost is not null)
        {
            RevealHighlight.UnregisterTemplateOverlay(_templateHost, this);
            _templateHost = null;
        }

        _localPointer = null;
        _wasLit = false;

        base.OnDetachedFromVisualTree(e);
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        _paletteValid = false;
        if (_wasLit)
            InvalidateVisual();
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        _paletteValid = false;
        if (_wasLit)
            InvalidateVisual();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsRevealEnabledProperty ||
            change.Property == ModeProperty ||
            change.Property == ToneProperty ||
            change.Property == CornerRadiusProperty ||
            change.Property == RevealBorderThicknessProperty ||
            change.Property == BorderLightRadiusProperty ||
            change.Property == HoverLightRadiusProperty ||
            change.Property == IsPressedStateProperty ||
            change.Property == IsVisibleProperty ||
            change.Property == IsEnabledProperty)
        {
            if (!IsEffectivelyActive())
                ClearLight();
            else if (_wasLit)
                InvalidateVisual();
        }
    }

    private Control? EffectiveHost => _adornedTarget ?? _templateHost;

    private static bool IsGloballyEnabled =>
        ThemeSettings.IsAnimationEnabled && ThemeSettings.IsRevealEffectEnabled;

    private bool IsEffectivelyActive()
    {
        if (!IsGloballyEnabled)
            return false;

        if (!IsEffectivelyVisible)
            return false;

        var host = EffectiveHost;
        if (host is not null)
        {
            if (!host.IsEffectivelyVisible || !host.IsEffectivelyEnabled)
                return false;

            if (host.IsSet(RevealHighlight.IsEnabledProperty))
            {
                if (!RevealHighlight.GetIsEnabled(host))
                    return false;
            }
            else if (!IsRevealEnabled)
            {
                return false;
            }
        }
        else if (!IsRevealEnabled || !IsEffectivelyEnabled)
        {
            return false;
        }

        return GetEffectiveMode() != RevealHighlightMode.None;
    }

    private RevealHighlightMode GetEffectiveMode()
    {
        var host = EffectiveHost;
        return host is not null && host.IsSet(RevealHighlight.ModeProperty)
            ? RevealHighlight.GetMode(host)
            : Mode;
    }

    private RevealHighlightTone GetEffectiveTone()
    {
        var host = EffectiveHost;
        return host is not null && host.IsSet(RevealHighlight.ToneProperty)
            ? RevealHighlight.GetTone(host)
            : Tone;
    }

    private double GetEffectiveBorderLightRadius()
    {
        var host = EffectiveHost;
        return host is not null && host.IsSet(RevealHighlight.BorderLightRadiusProperty)
            ? RevealHighlight.GetBorderLightRadius(host)
            : BorderLightRadius;
    }

    private double GetEffectiveHoverLightRadius()
    {
        var host = EffectiveHost;
        return host is not null && host.IsSet(RevealHighlight.HoverLightRadiusProperty)
            ? RevealHighlight.GetHoverLightRadius(host)
            : HoverLightRadius;
    }

    private double GetEffectiveBorderThickness()
    {
        var host = EffectiveHost;
        return host is not null && host.IsSet(RevealHighlight.BorderThicknessProperty)
            ? RevealHighlight.GetBorderThickness(host)
            : RevealBorderThickness;
    }

    private CornerRadius GetEffectiveCornerRadius()
    {
        var host = EffectiveHost;
        if (host is not null && host.IsSet(RevealHighlight.CornerRadiusProperty))
        {
            var overrideRadius = RevealHighlight.GetCornerRadius(host);
            if (overrideRadius.HasValue)
                return overrideRadius.Value;
        }

        if (_adornedTarget is Border border)
            return border.CornerRadius;

        if (_adornedTarget is TemplatedControl templated)
            return templated.CornerRadius;

        return CornerRadius;
    }

    internal void UpdatePointerFromTopLevel(
        TopLevel topLevel,
        Point topLevelPos,
        Visual? pointerVisual,
        Visual? pointerScope)
    {
        if (!IsEffectivelyActive() || Bounds.Width <= 0.5 || Bounds.Height <= 0.5)
        {
            ClearLight();
            return;
        }

        var local = topLevel.TranslatePoint(topLevelPos, this);
        if (!local.HasValue)
        {
            ClearLight();
            return;
        }

        var p = local.Value;
        var borderRadius = GetEffectiveBorderLightRadius();
        var hoverRadius = GetEffectiveHoverLightRadius();
        var maxRadius = Math.Max(borderRadius, hoverRadius) + 8;

        if (p.X >= -maxRadius && p.X <= Bounds.Width + maxRadius &&
            p.Y >= -maxRadius && p.Y <= Bounds.Height + maxRadius)
        {
            var targetVisual = (Visual?)EffectiveHost ?? this;

            // 1. Scope isolation: keep Reveal strictly within the active overlay/drawer/popup scope.
            var targetScope = FindRevealScope(targetVisual, topLevel);
            if (!ReferenceEquals(targetScope, pointerScope))
            {
                ClearLight();
                return;
            }

            // 2. Skip non-interactive or faded-out ancestor subtrees (e.g. inactive pages or closing drawers).
            if (!IsAncestorChainInteractive(topLevel))
            {
                ClearLight();
                return;
            }

            // 3. Automatic Z-order visual occlusion check at the closest inset point on the control.
            var width = Bounds.Width;
            var height = Bounds.Height;
            var insetX = Math.Min(2.0, width * 0.5);
            var insetY = Math.Min(2.0, height * 0.5);
            var clampedX = Math.Clamp(p.X, insetX, width - insetX);
            var clampedY = Math.Clamp(p.Y, insetY, height - insetY);

            Visual? probeHit;
            if (Math.Abs(clampedX - p.X) < 0.01 && Math.Abs(clampedY - p.Y) < 0.01)
            {
                probeHit = pointerVisual;
            }
            else
            {
                var probeTopLevel = this.TranslatePoint(new Point(clampedX, clampedY), topLevel);
                probeHit = probeTopLevel.HasValue ? topLevel.InputHitTest(probeTopLevel.Value) as Visual : null;
            }

            if (probeHit is not null && IsVisualInFrontOf(probeHit, targetVisual))
            {
                ClearLight();
                return;
            }

            _localPointer = p;
            _wasLit = true;
            InvalidateVisual();
        }
        else if (_wasLit)
        {
            ClearLight();
        }
    }

    private bool IsAncestorChainInteractive(TopLevel topLevel)
    {
        Visual? current = EffectiveHost ?? this.GetVisualParent();
        while (current is not null && !ReferenceEquals(current, topLevel))
        {
            if (!current.IsVisible || current.Opacity <= 0.01)
                return false;

            if (current is InputElement input && !input.IsHitTestVisible)
                return false;

            current = current.GetVisualParent();
        }

        return true;
    }

    private static Visual? FindRevealScope(Visual? visual, TopLevel topLevel)
    {
        var current = visual;
        while (current is not null && !ReferenceEquals(current, topLevel))
        {
            if (current is OverlayPopupHost)
                return current;

            if (current is Control control &&
                control.IsEffectivelyVisible &&
                control.IsHitTestVisible &&
                RevealHighlight.GetIsScope(control))
            {
                return control;
            }

            var parent = current.GetVisualParent();
            if (parent is OverlayLayer)
                return current;

            if (parent is VisualLayerManager vlm &&
                !ReferenceEquals(current, vlm.Child) &&
                current is not AdornerLayer &&
                current is not OverlayLayer)
            {
                return current;
            }

            current = parent;
        }

        return null;
    }

    private static bool IsVisualInFrontOf(Visual candidate, Visual reference)
    {
        if (ReferenceEquals(candidate, reference))
            return false;

        var depthC = 0;
        for (var cur = candidate.GetVisualParent(); cur is not null; cur = cur.GetVisualParent())
        {
            if (ReferenceEquals(cur, reference))
                return false;
            depthC++;
        }

        var depthR = 0;
        for (var cur = reference.GetVisualParent(); cur is not null; cur = cur.GetVisualParent())
        {
            if (ReferenceEquals(cur, candidate))
                return false;
            depthR++;
        }

        Visual? c = candidate;
        Visual? r = reference;
        while (depthC > depthR)
        {
            c = c?.GetVisualParent();
            depthC--;
        }

        while (depthR > depthC)
        {
            r = r?.GetVisualParent();
            depthR--;
        }

        while (c is not null && r is not null)
        {
            var parentC = c.GetVisualParent();
            var parentR = r.GetVisualParent();
            if (ReferenceEquals(parentC, parentR))
            {
                if (parentC is null)
                    return false;

                if (c.ZIndex != r.ZIndex)
                    return c.ZIndex > r.ZIndex;

                foreach (var child in parentC.GetVisualChildren())
                {
                    if (ReferenceEquals(child, r))
                        return true;
                    if (ReferenceEquals(child, c))
                        return false;
                }

                return false;
            }

            c = parentC;
            r = parentR;
        }

        return false;
    }

    internal void ClearLight()
    {
        if (!_wasLit && !_localPointer.HasValue)
            return;

        _localPointer = null;
        _wasLit = false;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        if (_localPointer is not { } localPos || !IsEffectivelyActive())
            return;

        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0.5 || height <= 0.5)
            return;

        var mode = GetEffectiveMode();
        if (mode == RevealHighlightMode.None)
            return;

        var rect = new Rect(0, 0, width, height);

        // In adorner mode (RevealHighlight.IsEnabled on arbitrary Borders/panels), intersect all
        // ClipToBounds visual ancestors (e.g. ScrollViewer viewports) and honor ancestor Opacity
        // so Reveal never bleeds outside scroll containers or shows on faded-out elements.
        DrawingContext.PushedState? ancestorClip = null;
        DrawingContext.PushedState? ancestorOpacity = null;
        if (_adornedTarget is not null)
        {
            if (!TryComputeAdornerVisibility(_adornedTarget, rect, out var visibleRect, out var effectiveOpacity))
                return;

            if (visibleRect != rect)
                ancestorClip = context.PushClip(visibleRect);
            if (effectiveOpacity < 0.999)
                ancestorOpacity = context.PushOpacity(effectiveOpacity);
        }

        try
        {
            var palette = ResolvePalette();
            var tone = GetEffectiveTone();
            var toneScale = tone == RevealHighlightTone.Subtle ? 0.65 : 1.0;

            Color borderCore;
            Color borderMid;
            Color hoverCore;
            Color hoverMid;
            Color pressedColor;

            if (tone == RevealHighlightTone.Accent)
            {
                borderCore = palette.AccentBorderColor;
                borderMid = palette.AccentBorderMidColor;
                hoverCore = palette.AccentHoverColor;
                hoverMid = palette.AccentHoverMidColor;
                pressedColor = palette.AccentPressedColor;
            }
            else
            {
                borderCore = ScaleAlpha(palette.BorderColor, toneScale);
                borderMid = ScaleAlpha(palette.BorderMidColor, toneScale);
                hoverCore = ScaleAlpha(palette.HoverColor, toneScale);
                hoverMid = ScaleAlpha(palette.HoverMidColor, toneScale);
                pressedColor = ScaleAlpha(palette.PressedColor, toneScale);
            }

            var cornerRadius = GetEffectiveCornerRadius();
            var isInside = rect.Contains(localPos);

            // 1. Surface hover spotlight (clipped cleanly to CornerRadius)
            if (mode is RevealHighlightMode.BorderAndHover or RevealHighlightMode.HoverOnly && isInside)
            {
                var hoverRadius = Math.Max(24, GetEffectiveHoverLightRadius());
                var center = new RelativePoint(localPos, RelativeUnit.Absolute);
                var radiusScalar = new RelativeScalar(hoverRadius, RelativeUnit.Absolute);

                var effectiveHoverCore = IsPressedState ? pressedColor : hoverCore;
                var effectiveHoverMid = IsPressedState ? ScaleAlpha(pressedColor, 0.35) : hoverMid;

                IReadOnlyList<ImmutableGradientStop> hoverStops =
                [
                    new ImmutableGradientStop(0.0, effectiveHoverCore),
                    new ImmutableGradientStop(0.45, effectiveHoverMid),
                    new ImmutableGradientStop(1.0, Color.FromArgb(0, effectiveHoverCore.R, effectiveHoverCore.G, effectiveHoverCore.B))
                ];

                var hoverBrush = new ImmutableRadialGradientBrush(
                    gradientStops: hoverStops,
                    opacity: 1.0,
                    center: center,
                    gradientOrigin: center,
                    radiusX: radiusScalar,
                    radiusY: radiusScalar);

                var outerRounded = CreateClampedRoundedRect(rect, cornerRadius, 0);
                using (context.PushClip(outerRounded))
                {
                    context.DrawRectangle(hoverBrush, null, rect);
                }
            }

            // 2. Proximity border light (illuminates edges facing cursor even from nearby controls)
            if (mode is RevealHighlightMode.BorderAndHover or RevealHighlightMode.BorderOnly)
            {
                var borderRadius = Math.Max(24, GetEffectiveBorderLightRadius());
                var dist = DistanceToRect(localPos, rect);
                if (dist < borderRadius)
                {
                    var edgeFade = dist > borderRadius * 0.45
                        ? Math.Clamp(1.0 - (dist - borderRadius * 0.45) / (borderRadius * 0.55), 0.0, 1.0)
                        : 1.0;

                    var intensity = (isInside ? 1.0 : 0.72) * edgeFade;
                    if (intensity > 0.01)
                    {
                        var thickness = Math.Clamp(GetEffectiveBorderThickness(), 0.75, 3.0);
                        var inset = thickness * 0.5;
                        var strokeRect = rect.Deflate(inset);
                        if (strokeRect.Width > 0 && strokeRect.Height > 0)
                        {
                            var center = new RelativePoint(localPos, RelativeUnit.Absolute);
                            var radiusScalar = new RelativeScalar(borderRadius, RelativeUnit.Absolute);

                            IReadOnlyList<ImmutableGradientStop> borderStops =
                            [
                                new ImmutableGradientStop(0.0, borderCore),
                                new ImmutableGradientStop(0.48, borderMid),
                                new ImmutableGradientStop(1.0, Color.FromArgb(0, borderCore.R, borderCore.G, borderCore.B))
                            ];

                            var borderBrush = new ImmutableRadialGradientBrush(
                                gradientStops: borderStops,
                                opacity: intensity,
                                center: center,
                                gradientOrigin: center,
                                radiusX: radiusScalar,
                                radiusY: radiusScalar);

                            var pen = new ImmutablePen(borderBrush, thickness);
                            var strokeRounded = CreateClampedRoundedRect(strokeRect, cornerRadius, inset);
                            context.DrawRectangle(null, pen, strokeRounded);
                        }
                    }
                }
            }
        }
        finally
        {
            ancestorOpacity?.Dispose();
            ancestorClip?.Dispose();
        }
    }

    private bool TryComputeAdornerVisibility(
        Control target,
        Rect localBounds,
        out Rect clipRect,
        out double effectiveOpacity)
    {
        clipRect = localBounds;
        effectiveOpacity = Math.Clamp(target.Opacity, 0.0, 1.0);
        if (effectiveOpacity <= 0.01)
            return false;

        var current = target.GetVisualParent();

        while (current is not null)
        {
            if (current is TopLevel)
                break;

            effectiveOpacity *= Math.Clamp(current.Opacity, 0.0, 1.0);
            if (effectiveOpacity <= 0.01)
                return false;

            if (current.ClipToBounds && current.Bounds.Width > 0 && current.Bounds.Height > 0)
            {
                var topLeft = current.TranslatePoint(new Point(0, 0), this);
                var bottomRight = current.TranslatePoint(new Point(current.Bounds.Width, current.Bounds.Height), this);
                if (topLeft.HasValue && bottomRight.HasValue)
                {
                    var ancestorRect = new Rect(topLeft.Value, bottomRight.Value).Normalize();
                    clipRect = clipRect.Intersect(ancestorRect);
                    if (clipRect.Width <= 0.5 || clipRect.Height <= 0.5)
                        return false;
                }
            }

            current = current.GetVisualParent();
        }

        return clipRect.Width > 0.5 && clipRect.Height > 0.5;
    }

    private static double DistanceToRect(Point p, Rect rect)
    {
        var dx = p.X < rect.Left ? rect.Left - p.X : p.X > rect.Right ? p.X - rect.Right : 0;
        var dy = p.Y < rect.Top ? rect.Top - p.Y : p.Y > rect.Bottom ? p.Y - rect.Bottom : 0;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static RoundedRect CreateClampedRoundedRect(Rect rect, CornerRadius cr, double inset)
    {
        var maxR = Math.Min(rect.Width, rect.Height) * 0.5;
        var tl = Math.Clamp(cr.TopLeft - inset, 0, maxR);
        var tr = Math.Clamp(cr.TopRight - inset, 0, maxR);
        var br = Math.Clamp(cr.BottomRight - inset, 0, maxR);
        var bl = Math.Clamp(cr.BottomLeft - inset, 0, maxR);
        return new RoundedRect(rect, tl, tr, br, bl);
    }

    private static Color ScaleAlpha(Color c, double factor)
    {
        var alpha = (byte)Math.Clamp(Math.Round(c.A * factor), 0, 255);
        return Color.FromArgb(alpha, c.R, c.G, c.B);
    }

    private RevealPalette ResolvePalette()
    {
        var variant = ActualThemeVariant;
        if (_paletteValid && _cachedThemeVariant == variant)
            return _cachedPalette;

        var isDark = variant == ThemeVariant.Dark;

        var hoverColor = ResolveColor("ThemeX.Reveal.HoverColor", variant, isDark ? Color.Parse("#0DFFFFFF") : Color.Parse("#140078D4"));
        var hoverMidColor = ResolveColor("ThemeX.Reveal.HoverMidColor", variant, isDark ? Color.Parse("#04FFFFFF") : Color.Parse("#060078D4"));
        var pressedColor = ResolveColor("ThemeX.Reveal.PressedColor", variant, isDark ? Color.Parse("#16FFFFFF") : Color.Parse("#1E0078D4"));

        // On Light mode white/card surfaces (#FDFDFD), pure white hover highlights have near-zero
        // contrast while black smudges look muddy. Blend the active ThemeX Accent color into a
        // whisper-subtle surface sheen so hovered cards & buttons respond cleanly in Light mode.
        if (!isDark && TryResolveColor(ThemeSettings.AccentColorKey, variant, out var accent))
        {
            if (accent.GetGrayLevel() > 0.82)
                accent = accent.ToDarker();

            hoverColor = Color.FromArgb(0x14, accent.R, accent.G, accent.B);
            hoverMidColor = Color.FromArgb(0x06, accent.R, accent.G, accent.B);
            pressedColor = Color.FromArgb(0x1E, accent.R, accent.G, accent.B);
        }

        _cachedPalette = new RevealPalette(
            ResolveColor("ThemeX.Reveal.BorderColor", variant, isDark ? Color.Parse("#38FFFFFF") : Color.Parse("#24000000")),
            ResolveColor("ThemeX.Reveal.BorderMidColor", variant, isDark ? Color.Parse("#16FFFFFF") : Color.Parse("#0E000000")),
            hoverColor,
            hoverMidColor,
            pressedColor,
            ResolveColor("ThemeX.Reveal.AccentBorderColor", variant, Color.Parse("#48FFFFFF")),
            ResolveColor("ThemeX.Reveal.AccentBorderMidColor", variant, Color.Parse("#1AFFFFFF")),
            ResolveColor("ThemeX.Reveal.AccentHoverColor", variant, isDark ? Color.Parse("#14FFFFFF") : Color.Parse("#16FFFFFF")),
            ResolveColor("ThemeX.Reveal.AccentHoverMidColor", variant, isDark ? Color.Parse("#05FFFFFF") : Color.Parse("#06FFFFFF")),
            ResolveColor("ThemeX.Reveal.AccentPressedColor", variant, Color.Parse("#1EFFFFFF")));

        _cachedThemeVariant = variant;
        _paletteValid = true;
        return _cachedPalette;
    }

    private bool TryResolveColor(string key, ThemeVariant? variant, out Color color)
    {
        if (this.TryFindResource(key, variant, out var value))
        {
            if (value is Color c)
            {
                color = c;
                return true;
            }

            if (value is ISolidColorBrush brush)
            {
                color = brush.Color;
                return true;
            }
        }

        color = default;
        return false;
    }

    private Color ResolveColor(string key, ThemeVariant? variant, Color fallback)
    {
        return TryResolveColor(key, variant, out var resolved) ? resolved : fallback;
    }

    private readonly record struct RevealPalette(
        Color BorderColor,
        Color BorderMidColor,
        Color HoverColor,
        Color HoverMidColor,
        Color PressedColor,
        Color AccentBorderColor,
        Color AccentBorderMidColor,
        Color AccentHoverColor,
        Color AccentHoverMidColor,
        Color AccentPressedColor);

    /// <summary>
    ///     Lightweight per-<see cref="TopLevel" /> pointer proximity broadcaster.
    ///     Automatically attaches on first registered overlay and detaches all handlers when empty.
    /// </summary>
    private sealed class RevealTracker
    {
        private static readonly ConditionalWeakTable<TopLevel, RevealTracker> Trackers = new();
        private static readonly List<WeakReference<RevealTracker>> ActiveTrackers = [];

        private readonly TopLevel _topLevel;
        private readonly HashSet<RevealOverlay> _overlays = [];
        private Point? _lastTopLevelPos;
        private WeakReference<Visual>? _lastPointerScopeRef;
        private bool _attached;
        private bool _wheelRefreshQueued;

        static RevealTracker()
        {
            ThemeSettings.AnimationEnabledChanged += OnGlobalRevealStateChanged;
            ThemeSettings.RevealEffectEnabledChanged += OnGlobalRevealStateChanged;
        }

        private RevealTracker(TopLevel topLevel)
        {
            _topLevel = topLevel;
        }

        public static RevealTracker GetOrCreate(TopLevel topLevel)
        {
            if (Trackers.TryGetValue(topLevel, out var existing))
                return existing;

            var created = new RevealTracker(topLevel);
            Trackers.Add(topLevel, created);
            for (var i = ActiveTrackers.Count - 1; i >= 0; i--)
            {
                if (!ActiveTrackers[i].TryGetTarget(out _))
                    ActiveTrackers.RemoveAt(i);
            }
            ActiveTrackers.Add(new WeakReference<RevealTracker>(created));
            return created;
        }

        public static RevealTracker? TryGet(TopLevel topLevel)
        {
            return Trackers.TryGetValue(topLevel, out var tracker) ? tracker : null;
        }

        public void Register(RevealOverlay overlay)
        {
            if (_overlays.Add(overlay) && !_attached)
                AttachHandlers();
        }

        public void Unregister(RevealOverlay overlay)
        {
            if (!_overlays.Remove(overlay))
                return;

            if (_overlays.Count == 0 && _attached)
                DetachHandlers();
        }

        public void InvalidateScope()
        {
            _lastTopLevelPos = null;
            _lastPointerScopeRef = null;
            ClearAll();
        }

        private void AttachHandlers()
        {
            _attached = true;
            _topLevel.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
            _topLevel.AddHandler(InputElement.PointerExitedEvent, OnPointerExited, RoutingStrategies.Direct, handledEventsToo: true);
            _topLevel.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel, handledEventsToo: true);

            if (_topLevel is Window window)
                window.Deactivated += OnWindowDeactivated;
        }

        private void DetachHandlers()
        {
            _attached = false;
            _lastTopLevelPos = null;
            _lastPointerScopeRef = null;
            _topLevel.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
            _topLevel.RemoveHandler(InputElement.PointerExitedEvent, OnPointerExited);
            _topLevel.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);

            if (_topLevel is Window window)
                window.Deactivated -= OnWindowDeactivated;
        }

        private Visual? ResolvePointerVisual(object? eventSource, Point topLevelPos)
        {
            if (_topLevel.InputHitTest(topLevelPos) is Visual hitVisual)
                return hitVisual;

            if (eventSource is Visual sourceVisual && sourceVisual.IsAttachedToVisualTree())
                return sourceVisual;

            return null;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!IsGloballyEnabled)
            {
                ClearAll();
                return;
            }

            var pos = e.GetPosition(_topLevel);
            var pointerVisual = ResolvePointerVisual(e.Source, pos);
            var pointerScope = FindRevealScope(pointerVisual, _topLevel);

            Visual? lastScope = null;
            _lastPointerScopeRef?.TryGetTarget(out lastScope);

            if (_lastTopLevelPos is { } prev &&
                ReferenceEquals(lastScope, pointerScope) &&
                Math.Abs(pos.X - prev.X) < 0.75 &&
                Math.Abs(pos.Y - prev.Y) < 0.75)
            {
                return;
            }

            _lastTopLevelPos = pos;
            if (!ReferenceEquals(lastScope, pointerScope))
                _lastPointerScopeRef = pointerScope is not null ? new WeakReference<Visual>(pointerScope) : null;

            BroadcastPointer(pos, pointerVisual, pointerScope);
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (!IsGloballyEnabled)
                return;

            _lastTopLevelPos = e.GetPosition(_topLevel);
            if (_wheelRefreshQueued)
                return;

            _wheelRefreshQueued = true;
            Dispatcher.UIThread.Post(() =>
            {
                _wheelRefreshQueued = false;
                if (_attached && IsGloballyEnabled && _lastTopLevelPos is { } pos)
                {
                    var pointerVisual = ResolvePointerVisual(null, pos);
                    var pointerScope = FindRevealScope(pointerVisual, _topLevel);
                    _lastPointerScopeRef = pointerScope is not null ? new WeakReference<Visual>(pointerScope) : null;
                    BroadcastPointer(pos, pointerVisual, pointerScope);
                }
            }, DispatcherPriority.Render);
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            _lastTopLevelPos = null;
            _lastPointerScopeRef = null;
            ClearAll();
        }

        private void OnWindowDeactivated(object? sender, EventArgs e)
        {
            _lastTopLevelPos = null;
            _lastPointerScopeRef = null;
            ClearAll();
        }

        private void BroadcastPointer(Point topLevelPos, Visual? pointerVisual, Visual? pointerScope)
        {
            foreach (var overlay in _overlays)
                overlay.UpdatePointerFromTopLevel(_topLevel, topLevelPos, pointerVisual, pointerScope);
        }

        private void ClearAll()
        {
            foreach (var overlay in _overlays)
                overlay.ClearLight();
        }

        private static void OnGlobalRevealStateChanged(bool enabled)
        {
            if (IsGloballyEnabled)
                return;

            for (var i = ActiveTrackers.Count - 1; i >= 0; i--)
            {
                if (ActiveTrackers[i].TryGetTarget(out var tracker))
                {
                    tracker._lastTopLevelPos = null;
                    tracker._lastPointerScopeRef = null;
                    tracker.ClearAll();
                }
                else
                {
                    ActiveTrackers.RemoveAt(i);
                }
            }
        }
    }
}
