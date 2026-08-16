using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThemeX.StyledProp.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class Main : IIncrementalGenerator
    {
        private const string GenerateAttribute = "ThemeX.SourceGenerator.Shared.StyledPropAttribute";
        private const string GenerateDirectAttribute = "ThemeX.SourceGenerator.Shared.DirectPropAttribute";
        private static readonly SymbolDisplayFormat FullyQualifiedNullableFormat =
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var sourceData = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (syntaxNode, _) => IsCandidateProperty(syntaxNode),
                    (gsc, _) => GetClassWithStyledProps(gsc))
                .Where(static classInfo => classInfo != null)
                .Collect();

            context.RegisterSourceOutput(sourceData, (spc, classInfoList) =>
            {
                foreach (var classInfo in Enumerable.OfType<ClassInfo>(classInfoList))
                {
                    var (className, source) = GenerateSourceData(classInfo);

                    foreach (var diagnostic in classInfo.Diagnostics) spc.ReportDiagnostic(diagnostic);

                    spc.AddSource($"{className}.StyledProp.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            });
        }

        private static AttributeListSyntax GetGeneratedCodeAttribute()
        {
            return AttributeList(SingletonSeparatedList(
                Attribute(ParseName("System.CodeDom.Compiler.GeneratedCodeAttribute"))
                    .WithArgumentList(AttributeArgumentList(
                        SeparatedList(new[]
                        {
                            AttributeArgument(
                                LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal("StyledPropGenerator"))
                            ),
                            AttributeArgument(
                                LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal("1.0"))
                            )
                        })
                    ))));
        }

        private static bool IsCandidateProperty(SyntaxNode node)
        {
            // Check if the node is a ClassDeclarationSyntax with properties
            if (node is not ClassDeclarationSyntax classDeclaration)
                return false;

            // Check if any property in the class has one of the supported attributes.
            return classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                .Any(property => property.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Any(attr => attr.Name.ToString().Contains("StyledProp") ||
                                 attr.Name.ToString().Contains("DirectProp")));
        }

        private static ClassInfo? GetClassWithStyledProps(GeneratorSyntaxContext context)
        {
            // Ensure the node is a ClassDeclarationSyntax
            if (context.Node is not ClassDeclarationSyntax classDeclaration)
                return null;

            // Retrieve the class symbol
            if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not { } classSymbol)
                return null;

            // Initialize a ClassInfo object
            var classInfo = new ClassInfo
            {
                Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
                ClassName = classSymbol.Name,
                Properties = new List<PropertyDetail>()
            };

            // Iterate through all properties in the class
            foreach (var member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
            {
                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(member);

                // Check if the property has a supported property-generation attribute.
                var generatedPropAttribute = propertySymbol?.GetAttributes()
                    .FirstOrDefault(attr =>
                        attr.AttributeClass?.ToDisplayString() == GenerateAttribute ||
                        attr.AttributeClass?.ToDisplayString() == GenerateDirectAttribute);

                if (generatedPropAttribute == null)
                    continue;

                var isDirect = generatedPropAttribute.AttributeClass?.ToDisplayString() == GenerateDirectAttribute;

                var semanticModel = context.SemanticModel;
                var attribute = member.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .First(candidate =>
                    {
                        var constructor = semanticModel.GetSymbolInfo(candidate).Symbol as IMethodSymbol;
                        return constructor?.ContainingType.ToDisplayString() is GenerateAttribute or GenerateDirectAttribute;
                    });

                // Determine the target class from the attribute argument
                var targetClassType = generatedPropAttribute.NamedArguments
                    .FirstOrDefault(arg => arg.Key == "TargetClass")
                    .Value.Value as INamedTypeSymbol;

                var bindingMode = generatedPropAttribute.NamedArguments
                    .FirstOrDefault(arg => arg.Key == "BindingMode")
                    .Value.Value as string;

                var isLangKey = generatedPropAttribute.NamedArguments
                    .FirstOrDefault(arg => arg.Key == "IsLangKey")
                    .Value.Value as bool?;

                var rawDefaultValue = generatedPropAttribute.NamedArguments
                    .FirstOrDefault(arg => arg.Key == "DefaultValue")
                    .Value.Value;

                var defaultValueName = attribute.ArgumentList?.Arguments
                    .FirstOrDefault(arg => arg.NameEquals?.Name.Identifier.Text == "DefaultValue");

                string? defaultValueFullName = null;

                if (defaultValueName?.Expression is InvocationExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax { Identifier: { Text: "nameof" } }
                    } invocation)
                {
                    // Resolve the symbol for the nameof expression
                    var nameofArgument = invocation.ArgumentList.Arguments.First().Expression;
                    var nameofSymbol = semanticModel.GetSymbolInfo(nameofArgument).Symbol;
                    var fallbackExpr = nameofArgument.ToFullString();

                    if (nameofSymbol is IFieldSymbol fieldSymbol)
                    {
                        defaultValueFullName =
                            $"{fieldSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{fieldSymbol.Name}";
                    }
                    else if (nameofSymbol is IPropertySymbol property &&
                             nameofArgument is MemberAccessExpressionSyntax memberAccess)
                    {
                        if (!property.IsStatic)
                        {
                            var parentExpr = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                            var instanceProperty = semanticModel.GetSymbolInfo(memberAccess).Symbol;

                            defaultValueFullName =
                                $"{parentExpr!.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{parentExpr.Name}.{instanceProperty!.Name}";
                        }
                        else
                        {
                            defaultValueFullName =
                                $"{property.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{property.Name}";
                        }
                    }
                    else if (!isLangKey.HasValue || !isLangKey.Value)
                    {
                        defaultValueFullName = fallbackExpr;
                        var location = defaultValueName.GetLocation();
                        var diagnostic = Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "STYLEDPROP001",
                                "可能的错误定义",
                                $"源生成器并没有找到 {fallbackExpr} 的定义，这可能是因为该表达式来自于其他的源生成器生成的内容。如果是使用 LangHelper 中的内容，请将 IsLangKey 设置为 true。",
                                "StyledProp",
                                DiagnosticSeverity.Error,
                                true),
                            location);

                        classInfo.Diagnostics.Add(diagnostic);
                    }
                }
                else
                {
                    if (defaultValueName != null && rawDefaultValue != null)
                    {
                        var exprSymbol = semanticModel.GetSymbolInfo(defaultValueName.Expression!).Symbol;
                        if (exprSymbol is IFieldSymbol fs)
                            defaultValueFullName =
                                $"{fs.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{fs.Name}";
                        else if (exprSymbol is IPropertySymbol ps)
                            defaultValueFullName =
                                $"{ps.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{ps.Name}";
                        else
                            defaultValueFullName = defaultValueName.Expression!.ToFullString();
                    }
                }

                var bindingModeStr = bindingMode switch
                {
                    "Default" => "global::Avalonia.Data.BindingMode.Default",
                    "OneWay" => "global::Avalonia.Data.BindingMode.OneWay",
                    "TwoWay" => "global::Avalonia.Data.BindingMode.TwoWay",
                    "OneTime" => "global::Avalonia.Data.BindingMode.OneTime",
                    "OneWayToSource" => "global::Avalonia.Data.BindingMode.OneWayToSource",
                    null => null,
                    _ => "global::Avalonia.Data.BindingMode.OneWay"
                };

                var defaultValueBinding = isLangKey switch
                {
                    true => $"global::LauncherX.UI.Theme.LangSwitcher.GetOneTimeStr(\"{rawDefaultValue}\")",
                    false => defaultValueFullName,
                    _ => defaultValueFullName
                };

                var setter = member.AccessorList?.Accessors
                    .FirstOrDefault(accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));

                // Add the property information to the ClassInfo object
                classInfo.Properties.Add(new PropertyDetail
                {
                    Name = propertySymbol!.Name,
                    Type = propertySymbol.Type.ToDisplayString(FullyQualifiedNullableFormat),
                    TargetClass = targetClassType?.ToDisplayString(),
                    BindingMode = bindingModeStr,
                    DefaultValue = defaultValueBinding,
                    IsDirect = isDirect,
                    HasSetter = setter != null,
                    SetterModifiers = setter == null
                        ? string.Empty
                        : string.Join(" ", setter.Modifiers.Select(modifier => modifier.Text))
                });
            }

            // Return null if no styled properties were found
            return classInfo.Properties.Count > 0 ? classInfo : null;
        }


        private static (string ClassName, string SourceText) GenerateSourceData(ClassInfo classInfo)
        {
            var sourceText = GeneratePropertyImplementation(classInfo);
            return (classInfo.ClassName, sourceText);
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateStyledProperty(ClassInfo info,
            PropertyDetail detail)
        {
            var styledType = detail.TargetClass ?? info.ClassName;
            var regArgs = new List<ArgumentSyntax>
            {
                Argument(
                    InvocationExpression(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind
                                        .NameOfKeyword,
                                    "nameof",
                                    "nameof",
                                    TriviaList())))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        IdentifierName(
                                            detail.Name)))))
                )
            };

            if (!string.IsNullOrEmpty(detail.DefaultValue))
                regArgs.Add(
                    Argument(
                            IdentifierName(detail.DefaultValue!))
                        .WithNameColon(
                            NameColon(
                                IdentifierName("defaultValue"))));

            if (!string.IsNullOrEmpty(detail.BindingMode))
                regArgs.Add(
                    Argument(
                            IdentifierName(detail.BindingMode!))
                        .WithNameColon(
                            NameColon(
                                IdentifierName("defaultBindingMode"))));

            yield return FieldDeclaration(
                    VariableDeclaration(
                            GenericName(
                                    Identifier("StyledProperty"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(detail.Type)))))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier($"{detail.Name}Property"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("AvaloniaProperty"),
                                                        GenericName(
                                                                Identifier("Register"))
                                                            .WithTypeArgumentList(
                                                                TypeArgumentList(
                                                                    SeparatedList<TypeSyntax>(
                                                                        new SyntaxNodeOrToken[]
                                                                        {
                                                                            IdentifierName(styledType),
                                                                            Token(SyntaxKind
                                                                                .CommaToken),
                                                                            IdentifierName(detail.Type)
                                                                        })))))
                                                .WithArgumentList(ArgumentList(SeparatedList(regArgs)))
                                        )
                                    )
                            )
                        )
                )
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword)))
                .AddAttributeLists(GetGeneratedCodeAttribute());

            yield return PropertyDeclaration(
                    IdentifierName(detail.Type),
                    Identifier(detail.Name))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                .WithAccessorList(
                    AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration)
                                    .WithExpressionBody(
                                        ArrowExpressionClause(
                                            InvocationExpression(
                                                    IdentifierName("GetValue"))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(
                                                                IdentifierName(
                                                                    $"{detail.Name}Property")))))))
                                    .WithSemicolonToken(
                                        Token(SyntaxKind.SemicolonToken)),
                                AccessorDeclaration(
                                        SyntaxKind.SetAccessorDeclaration)
                                    .WithExpressionBody(
                                        ArrowExpressionClause(
                                            InvocationExpression(
                                                    IdentifierName("SetValue"))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SeparatedList<ArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                Argument(
                                                                    IdentifierName(
                                                                        $"{detail.Name}Property")),
                                                                Token(SyntaxKind.CommaToken),
                                                                Argument(
                                                                    IdentifierName("value"))
                                                            })))))
                                    .WithSemicolonToken(
                                        Token(SyntaxKind.SemicolonToken))
                            })))
                .AddAttributeLists(GetGeneratedCodeAttribute());

            yield return MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier($"On{detail.Name}Changed"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PartialKeyword)))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .AddAttributeLists(GetGeneratedCodeAttribute());
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateDirectProperty(ClassInfo info,
            PropertyDetail detail)
        {
            var ownerType = detail.TargetClass ?? info.ClassName;
            var backingFieldName = $"__directProperty{detail.Name}";
            var initialValue = string.IsNullOrEmpty(detail.DefaultValue) ? "default!" : detail.DefaultValue!;

            yield return ParseMemberDeclaration($@"
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""StyledPropGenerator"", ""1.0"")]
private {detail.Type} {backingFieldName} = {initialValue};")!;

            var registrationArguments = new StringBuilder()
                .Append("nameof(").Append(detail.Name).Append("), ")
                .Append("static owner => owner.").Append(detail.Name);

            var hasPublicSetter = detail.HasSetter && string.IsNullOrEmpty(detail.SetterModifiers);
            if (hasPublicSetter)
                registrationArguments.Append(", static (owner, value) => owner.")
                    .Append(detail.Name).Append(" = value");

            if (!string.IsNullOrEmpty(detail.DefaultValue))
                registrationArguments.Append(", unsetValue: ").Append(detail.DefaultValue);

            if (!string.IsNullOrEmpty(detail.BindingMode))
                registrationArguments.Append(", defaultBindingMode: ").Append(detail.BindingMode);

            yield return ParseMemberDeclaration($@"
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""StyledPropGenerator"", ""1.0"")]
public static readonly global::Avalonia.DirectProperty<{ownerType}, {detail.Type}> {detail.Name}Property =
    global::Avalonia.AvaloniaProperty.RegisterDirect<{ownerType}, {detail.Type}>(
        {registrationArguments});")!;

            var setterImplementation = detail.HasSetter
                ? $"{detail.SetterModifiers} set => SetAndRaise({detail.Name}Property, ref {backingFieldName}, value);"
                : string.Empty;

            yield return ParseMemberDeclaration($@"
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""StyledPropGenerator"", ""1.0"")]
public partial {detail.Type} {detail.Name}
{{
    get => {backingFieldName};
    {setterImplementation}
}}")!;

            yield return ParseMemberDeclaration($@"
[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""StyledPropGenerator"", ""1.0"")]
partial void On{detail.Name}Changed();")!;
        }

        private static IfStatementSyntax GeneratePropertyChangedCheckStatement(PropertyDetail detail)
        {
            return IfStatement(
                BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("change"),
                        IdentifierName("Property")),
                    IdentifierName($"{detail.Name}Property")),
                ExpressionStatement(
                    InvocationExpression(
                        IdentifierName($"On{detail.Name}Changed"))));
        }

        private static MemberDeclarationSyntax GeneratePropertyChangedMethod(ClassInfo info)
        {
            var list = new List<StatementSyntax>();

            list.Add(ExpressionStatement(
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            BaseExpression(),
                            IdentifierName("OnPropertyChanged")))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    IdentifierName("change")))))));
            list.AddRange(info.Properties.Select(GeneratePropertyChangedCheckStatement));

            return MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("OnPropertyChanged"))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("change"))
                                .WithType(
                                    IdentifierName("AvaloniaPropertyChangedEventArgs")))))
                .WithBody(Block(list))
                .AddAttributeLists(GetGeneratedCodeAttribute());
        }

        private static string GeneratePropertyImplementation(ClassInfo classInfo)
        {
            var classMembers = new List<MemberDeclarationSyntax>();
            classMembers.AddRange(classInfo.Properties.SelectMany(detail => detail.IsDirect
                ? GenerateDirectProperty(classInfo, detail)
                : GenerateStyledProperty(classInfo, detail)));
            classMembers.Add(GeneratePropertyChangedMethod(classInfo));

            var generatedClass = CompilationUnit()
                .WithUsings(
                    List(
                        new[]
                        {
                            UsingDirective(
                                IdentifierName("Avalonia")),
                            UsingDirective(
                                QualifiedName(
                                    IdentifierName("Avalonia"),
                                    IdentifierName("Controls")))
                        }))
                .AddMembers(
                    NamespaceDeclaration(ParseName(classInfo.Namespace))
                        .AddMembers(
                            ClassDeclaration(classInfo.ClassName)
                                .WithModifiers(
                                    TokenList(
                                        Token(SyntaxKind.PartialKeyword)))
                                .WithMembers(List(classMembers))
                                .AddAttributeLists(GetGeneratedCodeAttribute())
                        ))
                .NormalizeWhitespace();

            // Nullable annotations in the generated property types (for example
            // ObservableCollection<T>?) need an explicit nullable context in the
            // generated file itself. The consuming project's nullable setting is
            // not inherited by source-generated trees.
            return "#nullable enable\n" + generatedClass.ToFullString();
        }
    }
}
