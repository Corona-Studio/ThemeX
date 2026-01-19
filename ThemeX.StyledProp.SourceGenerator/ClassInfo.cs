using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ThemeX.StyledProp.SourceGenerator
{
    public class ClassInfo
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public List<PropertyDetail> Properties { get; set; } = new();
        public List<Diagnostic> Diagnostics { get; set; } = new();
    }
}