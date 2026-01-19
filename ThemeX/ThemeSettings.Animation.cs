using Avalonia.Animation.Easings;

namespace ThemeX;

public partial class ThemeSettings
{
    public const string EnableAnimationKey = "ThemeX.Animation.IsEnabled";
    public const string AnimationDurationKey = "ThemeX.Animation.Duration";
    public const string AnimationEasingKey = "ThemeX.Animation.Easing";
    public const string FluentScrollKey = "ThemeX.Animation.EnableFluentScroll";
    private TimeSpan _animationDuration = TimeSpan.FromMilliseconds(450);
    private Easing _animationEasing = new SplineEasing(0.1, 0.9, 0.2);

    private bool _enableAnimation;

    public static TimeSpan AnimationDuration { get; private set; }
    public static Easing AnimationEasing { get; private set; } = null!;

    public void SetEnableAnimation(bool value)
    {
        _enableAnimation = value;

        _resourceDic[EnableAnimationKey] = value;
        _resourceDic[AnimationDurationKey] = value ? _animationDuration : TimeSpan.FromTicks(1);
        _resourceDic[AnimationEasingKey] = value ? _animationEasing : new LinearEasing();

        RaiseResourcesChanged();
    }

    public void SetAnimationDuration(TimeSpan value)
    {
        _animationDuration = value;
        AnimationDuration = value;

        if (_enableAnimation)
        {
            _resourceDic[AnimationDurationKey] = value;
            RaiseResourcesChanged();
        }
    }

    public void SetAnimationEasing(Easing value)
    {
        _animationEasing = value;
        AnimationEasing = value;

        if (_enableAnimation)
        {
            _resourceDic[AnimationEasingKey] = value;
            RaiseResourcesChanged();
        }
    }

    public void SetEnableFluentScroll(bool value)
    {
        _resourceDic[FluentScrollKey] = value;
        RaiseResourcesChanged();
    }
}