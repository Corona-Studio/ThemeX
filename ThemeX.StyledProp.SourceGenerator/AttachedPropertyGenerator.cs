using System.Linq;
using Corona.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ThemeX.StyledProp.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class AttachedPropertyGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor Invalid = new("STYLEDPROP002", "Invalid attached property",
        "Cannot generate attached property: {0}", "StyledProp", DiagnosticSeverity.Error, true);
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                                  SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var results = context.SyntaxProvider.ForAttributeWithMetadataName("ThemeX.SourceGenerator.Shared.AttachedPropAttribute",
            static (node, _) => node is MethodDeclarationSyntax, static (ctx, token) =>
            {
                var method = (IMethodSymbol)ctx.TargetSymbol;
                var type = method.ContainingType;
                for (var current = type; current is not null; current = current.ContainingType)
                    if (!current.IsPartial(token)) return Error("owner and containing types must be partial");
                if (!method.IsPartialDefinition || !method.IsStatic || method.DeclaredAccessibility != Accessibility.Public ||
                    !method.Name.StartsWith("Get", System.StringComparison.Ordinal) || method.Name.Length == 3 ||
                    method.Parameters.Length != 1 || method.Parameters[0].RefKind != RefKind.None || method.ReturnsVoid ||
                    method.IsGenericMethod || method.Parameters[0].Type is not INamedTypeSymbol target ||
                    (target.ToDisplayString() != "Avalonia.AvaloniaObject" && !target.InheritsFrom("Avalonia.AvaloniaObject")))
                    return Error("declare a public static partial GetName(AvaloniaObject target) method");
                var attribute = ctx.Attributes[0];
                var name = attribute.ConstructorArguments.FirstOrDefault().Value as string ?? method.Name.Substring(3);
                if (!CSharpNames.IsIdentifier(name) || type.GetMembers(name + "Property").Length > 0)
                    return Error("invalid or conflicting property name " + name);
                var defaultValue = "default";
                if (attribute.NamedArguments.Any(a => a.Key == "DefaultValue"))
                {
                    var value = Named("DefaultValue");
                    var syntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax(token);
                    var expression = syntax.ArgumentList!.Arguments.First(a => a.NameEquals?.Name.Identifier.Text == "DefaultValue").Expression;
                    if (!ctx.SemanticModel.ClassifyConversion(expression, method.ReturnType).IsImplicit)
                        return Error("default value is not assignable to " + method.ReturnType);
                    defaultValue = value.Kind == TypedConstantKind.Enum ?
                        "(" + value.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ")" +
                        System.Convert.ToString(value.Value, System.Globalization.CultureInfo.InvariantCulture) : value.ToCSharpString();
                }
                return new GenerationResult<AttachedModel>(new(TypeDeclarationModel.From(type),
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), name, method.Name, method.ReturnType.ToDisplayString(TypeFormat),
                    target.ToDisplayString(TypeFormat), CSharpNames.Identifier(method.Parameters[0].Name), defaultValue,
                    Named("Inherits").Value is true, Named("ValidateTarget").Value is true), default);
                TypedConstant Named(string key) => attribute.NamedArguments.FirstOrDefault(a => a.Key == key).Value;
                GenerationResult<AttachedModel> Error(string message) => new(null,
                    new(new[] { DiagnosticInfo.Create(Invalid, ctx.TargetNode.GetLocation(), message) }));
            });
        context.RegisterSourceOutput(results.SelectMany(static (r, _) => r.Diagnostics),
            static (output, diagnostic) => output.ReportDiagnostic(diagnostic.ToDiagnostic()));
        context.RegisterSourceOutput(results.Where(static r => r.Value is not null).Select(static (r, _) => r.Value!)
            .WithTrackingName("AttachedPropertyModels"), static (output, model) =>
            {
                var writer = new CodeWriter().Header();
                model.Owner.Open(writer);
                writer.Line($"public static readonly global::Avalonia.AttachedProperty<{model.Type}> {model.Name}Property =");
                writer.Line($"    global::Avalonia.AvaloniaProperty.RegisterAttached<{model.Target}, {model.Type}>({CSharpNames.Literal(model.Name)}, typeof({model.OwnerType}), defaultValue: {model.Default}, inherits: {(model.Inherits ? "true" : "false")});");
                writer.Line($"public static partial {model.Type} {CSharpNames.Identifier(model.Getter)}({model.Target} {model.Parameter}) => {model.Parameter}.GetValue({model.Name}Property);");
                writer.Open($"public static void {CSharpNames.Identifier("Set" + model.Getter.Substring(3))}({model.Target} target, {model.Type} value)");
                if (model.Validate) writer.Line("global::System.ArgumentNullException.ThrowIfNull(target);");
                writer.Line($"target.SetValue({model.Name}Property, value);");
                writer.Close();
                model.Owner.Close(writer);
                output.AddSource(new SourceFile(CSharpNames.HintName(model.Owner.Identity + "." + model.Name) + ".AttachedProperty.g.cs", writer.ToString()));
            });
    }

    private sealed record AttachedModel(TypeDeclarationModel Owner, string OwnerType, string Name, string Getter,
        string Type, string Target, string Parameter, string Default, bool Inherits, bool Validate);
}
