using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Corona.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ThemeX.StyledProp.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class Main : IIncrementalGenerator
{
    private const string Styled = "ThemeX.SourceGenerator.Shared.StyledPropAttribute";
    private const string Direct = "ThemeX.SourceGenerator.Shared.DirectPropAttribute";
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                                  SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    private static readonly DiagnosticDescriptor Invalid = new("STYLEDPROP001", "Invalid Avalonia property",
        "Cannot generate Avalonia property: {0}", "StyledProp", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var styled = context.SyntaxProvider.ForAttributeWithMetadataName(Styled,
            static (node, _) => node is PropertyDeclarationSyntax, static (ctx, token) => Extract(ctx, token)).Collect();
        var direct = context.SyntaxProvider.ForAttributeWithMetadataName(Direct,
            static (node, _) => node is PropertyDeclarationSyntax, static (ctx, token) => Extract(ctx, token)).Collect();
        var results = styled.Combine(direct).SelectMany(static (input, _) => input.Left.Concat(input.Right)
            .Distinct());
        context.RegisterSourceOutput(results.SelectMany(static (r, _) => r.Diagnostics),
            static (output, diagnostic) => output.ReportDiagnostic(diagnostic.ToDiagnostic()));
        context.RegisterSourceOutput(results.Where(static r => r.Value is not null).Select(static (r, _) => r.Value!)
            .WithTrackingName("AvaloniaPropertyModels"), static (output, model) => output.AddSource(Emit(model)));
    }

    private static GenerationResult<PropertyTarget> Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var type = context.TargetSymbol.ContainingType!;
        for (var current = type; current is not null; current = current.ContainingType)
            if (!current.IsPartial(token)) return Error("owner and containing types must be partial");
        if (!type.InheritsFrom("Avalonia.AvaloniaObject")) return Error("owner must inherit AvaloniaObject");
        var properties = new List<PropertyModel>();
        var hasOverride = type.GetMembers("OnPropertyChanged").OfType<IMethodSymbol>().Any(m => m.IsOverride);
        if (hasOverride && !type.GetMembers("OnPropertyChanged").OfType<IMethodSymbol>()
            .Where(m => m.IsOverride).SelectMany(m => m.DeclaringSyntaxReferences)
            .Any(r => r.GetSyntax(token).DescendantNodes().OfType<InvocationExpressionSyntax>().Any(call =>
                call.Expression is IdentifierNameSyntax { Identifier.Text: "HandleGeneratedPropertyChanged" } or
                    MemberAccessExpressionSyntax { Name.Identifier.Text: "HandleGeneratedPropertyChanged" })))
            return Error("existing OnPropertyChanged override must call HandleGeneratedPropertyChanged(change)");
        foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
        {
            token.ThrowIfCancellationRequested();
            var attributes = property.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() is Styled or Direct).ToArray();
            if (attributes.Length == 0) continue;
            if (attributes.Length != 1) return Error(property.Name + " cannot be both styled and direct");
            var attribute = attributes[0];
            var syntax = (PropertyDeclarationSyntax)property.DeclaringSyntaxReferences[0].GetSyntax(token);
            var partial = syntax.Modifiers.Any(SyntaxKind.PartialKeyword);
            if (property.IsStatic || property.IsIndexer || property.DeclaredAccessibility != Accessibility.Public)
                return Error(property.Name + " must be a public instance property");
            if (type.GetMembers(property.Name + "Property").Length > 0)
                return Error(property.Name + "Property is reserved for generated registration");
            var isDirect = attribute.AttributeClass!.ToDisplayString() == Direct;
            if (!isDirect && partial && property.SetMethod is null)
                return Error(property.Name + " styled partial property requires a setter");
            var rawDefault = Named("DefaultValue");
            var defaultValue = attribute.NamedArguments.Any(a => a.Key == "DefaultValue") ? rawDefault.ToCSharpString() : null;
            if (rawDefault.Kind == TypedConstantKind.Enum)
                defaultValue = "(" + rawDefault.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ")" +
                    Convert.ToString(rawDefault.Value, System.Globalization.CultureInfo.InvariantCulture);
            var attrSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax(token);
            var defaultSyntax = attrSyntax.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "DefaultValue")?.Expression;
            var semanticModel = context.SemanticModel.Compilation.GetSemanticModel(syntax.SyntaxTree);
            if (defaultSyntax is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "nameof" } } invocation)
            {
                var reference = semanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[0].Expression, token).Symbol;
                if (reference is not (IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true }))
                    return Error(property.Name + " default reference must be a static field/property");
                if (!semanticModel.ClassifyConversion(invocation.ArgumentList.Arguments[0].Expression, property.Type).IsImplicit)
                    return Error(property.Name + " default reference is not assignable to its property type");
                defaultValue = reference.ContainingType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + CSharpNames.Identifier(reference.Name);
            }
            else if (defaultSyntax is not null && !semanticModel.ClassifyConversion(defaultSyntax, property.Type).IsImplicit)
                return Error(property.Name + " default is not assignable to its property type");
            var binding = Named("BindingMode").Value as string;
            if (binding is not (null or "Default" or "OneWay" or "TwoWay" or "OneTime" or "OneWayToSource"))
                return Error(property.Name + " has an invalid binding mode");
            var owner = (Named("TargetClass").Value as INamedTypeSymbol)?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        ?? type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var setter = syntax.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
            if (partial && setter is null && property.GetMethod is null) return Error("property requires a getter");
            var callback = !type.GetMembers("On" + property.Name + "Changed").OfType<IMethodSymbol>().Any(m =>
                m.DeclaringSyntaxReferences.Any(r => r.GetSyntax(token) is MethodDeclarationSyntax method &&
                    !method.Modifiers.Any(SyntaxKind.PartialKeyword)));
            properties.Add(new(property.Name, property.Type.ToDisplayString(TypeFormat), owner, isDirect,
                defaultValue, binding, partial, setter is not null,
                setter is null ? "" : string.Join(" ", setter.Modifiers.Select(m => m.Text)), callback));
            TypedConstant Named(string name) => attribute.NamedArguments.FirstOrDefault(a => a.Key == name).Value;
        }
        return new(new(TypeDeclarationModel.From(type), hasOverride, new(properties)), default);
        GenerationResult<PropertyTarget> Error(string message) => new(null,
            new(new[] { DiagnosticInfo.Create(Invalid, context.TargetNode.GetLocation(), message) }));
    }

    private static SourceFile Emit(PropertyTarget model)
    {
        var writer = new CodeWriter().Header();
        model.Type.Open(writer);
        foreach (var property in model.Properties)
        {
            var name = CSharpNames.Identifier(property.Name);
            var field = property.Name + "Property";
            var parameters = "nameof(" + name + ")";
            if (property.Direct)
            {
                parameters += ", static owner => owner." + name;
                if (property.HasSetter && property.SetterModifiers.Length == 0)
                    parameters += ", static (owner, value) => owner." + name + " = value";
            }
            if (property.Default is not null) parameters += (property.Direct ? ", unsetValue: " : ", defaultValue: ") + property.Default;
            if (property.Binding is not null) parameters += ", defaultBindingMode: global::Avalonia.Data.BindingMode." + property.Binding;
            var type = property.Direct ? $"global::Avalonia.DirectProperty<{property.Owner}, {property.Type}>" : $"global::Avalonia.StyledProperty<{property.Type}>";
            var register = property.Direct ? "RegisterDirect" : "Register";
            writer.Line($"public static readonly {type} {field} = global::Avalonia.AvaloniaProperty.{register}<{property.Owner}, {property.Type}>({parameters});");
            if (property.Partial)
            {
                var backing = "__directProperty" + property.Name;
                if (property.Direct) writer.Line($"private {property.Type} {backing} = {property.Default ?? "default!"};");
                writer.Open($"public partial {property.Type} {name}");
                writer.Line(property.Direct ? $"get => {backing};" : $"get => GetValue({field});");
                if (property.HasSetter)
                    writer.Line(property.SetterModifiers + (property.SetterModifiers.Length > 0 ? " " : "") +
                        (property.Direct ? $"set => SetAndRaise({field}, ref {backing}, value);" : $"set => SetValue({field}, value);"));
                writer.Close();
            }
            if (property.Callback) writer.Line($"partial void On{property.Name}Changed();");
        }
        if (!model.HasOverride)
        {
            writer.Open("protected override void OnPropertyChanged(global::Avalonia.AvaloniaPropertyChangedEventArgs change)");
            writer.Line("base.OnPropertyChanged(change);").Line("HandleGeneratedPropertyChanged(change);");
            writer.Close();
        }
        // Existing overrides can dispatch generated callbacks while retaining their own logic.
        writer.Open("private void HandleGeneratedPropertyChanged(global::Avalonia.AvaloniaPropertyChangedEventArgs change)");
        foreach (var property in model.Properties.Where(p => p.Callback))
            writer.Line($"if (change.Property == {property.Name}Property) On{property.Name}Changed();");
        writer.Close();
        model.Type.Close(writer);
        return new(CSharpNames.HintName(model.Type.Identity) + ".AvaloniaProperties.g.cs", writer.ToString());
    }

    private sealed record PropertyModel(string Name, string Type, string Owner, bool Direct, string? Default,
        string? Binding, bool Partial, bool HasSetter, string SetterModifiers, bool Callback);
    private sealed record PropertyTarget(TypeDeclarationModel Type, bool HasOverride, EquatableArray<PropertyModel> Properties);
}
