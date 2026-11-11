using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Rendering.Composition;

namespace ThemeX.Helpers;

public static class AnimationHelper
{
    public const string FinalValue = "this.FinalValue";
    public const string Opacity = "Opacity";
    public const string Offset = "Offset";
    public const string Scale = "Scale";
    public const string Size = "Size";

    extension(Visual control)
    {
        public void OnAttachedToVisualTreeOnce(Action action)
        {
            control.AttachedToVisualTree += AttachedToVisualTree;
            return;

            void AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
            {
                action();
                control.AttachedToVisualTree -= AttachedToVisualTree;
            }
        }
    }

    public static (CompositionVisual, ScalarKeyFrameAnimation) CreateCompositionFloatAnimation(
        Visual visual,
        float from,
        float to,
        TimeSpan duration,
        Easing? easing = null)
    {
        easing ??= ThemeSettings.AnimationEasing;

        var elementVisual = ElementComposition.GetElementVisual(visual)!;
        var compositor = elementVisual.Compositor;
        var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();

        opacityAnimation.InsertKeyFrame(0f, from, easing);
        opacityAnimation.InsertKeyFrame(1f, to, easing);
        opacityAnimation.Duration = duration;

        return (elementVisual, opacityAnimation);
    }

    public static (CompositionVisual, Vector3KeyFrameAnimation) CreateCompositionVector3Animation(
        Visual visual,
        Vector3 from,
        Vector3 to,
        TimeSpan duration,
        Easing? easing = null)
    {
        easing ??= ThemeSettings.AnimationEasing;

        var elementVisual = ElementComposition.GetElementVisual(visual)!;
        var compositor = elementVisual.Compositor;
        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();

        offsetAnimation.InsertKeyFrame(0f, from, easing);
        offsetAnimation.InsertKeyFrame(1f, to, easing);
        offsetAnimation.Duration = duration;

        return (elementVisual, offsetAnimation);
    }

    public static (CompositionVisual, ScalarKeyFrameAnimation) CreateAutoFinalEasingFloatAnimation(
        Visual visual,
        string property,
        TimeSpan? duration = null,
        Easing? easing = null)
    {
        duration ??= ThemeSettings.AnimationDuration;
        easing ??= ThemeSettings.AnimationEasing;

        var elementVisual = ElementComposition.GetElementVisual(visual)!;
        var compositor = elementVisual.Compositor;
        var animation = compositor.CreateScalarKeyFrameAnimation();

        animation.InsertExpressionKeyFrame(1f, FinalValue, easing);
        animation.Duration = duration.Value;
        animation.Target = property;

        return (elementVisual, animation);
    }

    public static (CompositionVisual, VectorKeyFrameAnimation) CreateAutoFinalEasingVectorAnimation(
        Visual visual,
        string property,
        TimeSpan? duration = null,
        Easing? easing = null)
    {
        duration ??= ThemeSettings.AnimationDuration;
        easing ??= ThemeSettings.AnimationEasing;

        var elementVisual = ElementComposition.GetElementVisual(visual)!;
        var compositor = elementVisual.Compositor;
        var animation = compositor.CreateVectorKeyFrameAnimation();

        animation.InsertExpressionKeyFrame(1f, FinalValue, easing);
        animation.Duration = duration.Value;
        animation.Target = property;

        return (elementVisual, animation);
    }

    public static (CompositionVisual, Vector3KeyFrameAnimation) CreateAutoFinalEasingVector3Animation(
        Visual visual,
        string property,
        TimeSpan? duration = null,
        Easing? easing = null)
    {
        duration ??= ThemeSettings.AnimationDuration;
        easing ??= ThemeSettings.AnimationEasing;

        var elementVisual = ElementComposition.GetElementVisual(visual)!;
        var compositor = elementVisual.Compositor;
        var animation = compositor.CreateVector3KeyFrameAnimation();

        animation.InsertExpressionKeyFrame(1f, FinalValue, easing);
        animation.Duration = duration.Value;
        animation.Target = property;

        return (elementVisual, animation);
    }
}