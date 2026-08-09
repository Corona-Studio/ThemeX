using System;

namespace ThemeX.SourceGenerator.Shared;

/// <summary>
///     Generates an Avalonia <c>StyledProperty</c> that participates in styles, animations,
///     inheritance, coercion, and value precedence.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StyledPropAttribute : Attribute
{
    public const string BindingDefault = "Default";
    public const string BindingOneWay = "OneWay";
    public const string BindingTwoWay = "TwoWay";
    public const string BindingOneTime = "OneTime";
    public const string BindingOneWayToSource = "OneWayToSource";

    /// <summary>
    ///     Specifies the target class type for the styled property.
    /// </summary>
    public Type? TargetClass { get; set; }

    public string BindingMode { get; set; } = BindingDefault;

    public object? DefaultValue { get; set; }

    public bool IsLangKey { get; set; } = false;
}
