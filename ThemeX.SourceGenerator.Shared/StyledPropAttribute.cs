using System;

namespace ThemeX.SourceGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StyledPropAttribute : Attribute
    {
        /// <summary>
        /// Specifies the target class type for the styled property.
        /// </summary>
        public Type? TargetClass { get; set; }

        public Avalonia.Data.BindingMode BindingMode { get; set; } = Avalonia.Data.BindingMode.Default;

        public object? DefaultValue { get; set; }

        public bool IsLangKey { get; set; } = false;
    }
}