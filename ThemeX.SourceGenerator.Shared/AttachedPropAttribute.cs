using System;

namespace ThemeX.SourceGenerator.Shared;

/// <summary>Generates an attached registration and Get/Set accessors from a partial getter.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AttachedPropAttribute(string? propertyName = null) : Attribute
{
    public string? PropertyName { get; } = propertyName;
    public object? DefaultValue { get; set; }
    public bool Inherits { get; set; }
    /// <summary>Retains setters that explicitly validated their target before migration.</summary>
    public bool ValidateTarget { get; set; }
}
