using System;

namespace ThemeX.SourceGenerator.Shared;

/// <summary>
///     Generates a field-backed Avalonia <c>DirectProperty</c>.
/// </summary>
/// <remarks>
///     Use this only for properties that need binding and change notification but must not
///     participate in styles, animations, inheritance, coercion, or value precedence. A
///     <c>private set</c> accessor generates a read-only DirectProperty registration.
///     A property with a body keeps its storage and only generates registration.
///     An existing OnPropertyChanged override must call HandleGeneratedPropertyChanged(change).
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DirectPropAttribute : Attribute
{
    public const string BindingDefault = "Default";
    public const string BindingOneWay = "OneWay";
    public const string BindingTwoWay = "TwoWay";
    public const string BindingOneTime = "OneTime";
    public const string BindingOneWayToSource = "OneWayToSource";

    public string BindingMode { get; set; } = BindingDefault;

    /// <summary>
    ///     Sets both the backing field's initial value and the direct property's unset value.
    /// </summary>
    public object? DefaultValue { get; set; }
}
