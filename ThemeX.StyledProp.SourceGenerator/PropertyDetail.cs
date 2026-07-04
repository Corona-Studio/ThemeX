using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ThemeX.StyledProp.SourceGenerator
{
    public class PropertyDetail
    {
        /// <summary>
        ///     Name of the property.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Fully qualified type name of the property.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        ///     Fully qualified target class name.
        /// </summary>
        public string? TargetClass { get; set; }

        /// <summary>
        ///     The original property syntax.
        /// </summary>
        public PropertyDeclarationSyntax PropertySyntax { get; set; } = null!;

        /// <summary>
        ///     Semantic type information for easier access.
        /// </summary>
        public ITypeSymbol? TypeSymbol { get; set; }

        /// <summary>
        ///     Binding mode for the property.
        /// </summary>
        public string? BindingMode { get; set; }

        /// <summary>
        ///     Default value for the property.
        /// </summary>
        public string? DefaultValue { get; set; }
    }
}