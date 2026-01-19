using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace ThemeX.StyledProp.SourceGenerator
{
    internal class StyledPropSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<ClassInfo> Properties { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertySyntax)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(propertySyntax) is not IPropertySymbol propertySymbol)
                return;

            var styledPropAttribute = propertySymbol.GetAttributes().FirstOrDefault(attr =>
                attr.AttributeClass?.Name == "StyledPropAttribute");

            if (styledPropAttribute == null)
                return;

            var targetType =
                styledPropAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "TargetClass").Value.Value as
                    INamedTypeSymbol;
            var targetClass = targetType?.ToDisplayString();

            var containingType = propertySymbol.ContainingType;
            var namespaceName = containingType.ContainingNamespace.ToDisplayString();

            var propertyInfo =
                Properties.FirstOrDefault(p => p.ClassName == containingType.Name && p.Namespace == namespaceName);
            if (propertyInfo == null)
            {
                propertyInfo = new ClassInfo { ClassName = containingType.Name, Namespace = namespaceName };
                Properties.Add(propertyInfo);
            }

            propertyInfo.Properties.Add(new PropertyDetail
            {
                Name = propertySymbol.Name,
                Type = propertySymbol.Type.ToDisplayString(),
                TargetClass = targetClass
            });
        }
    }
}