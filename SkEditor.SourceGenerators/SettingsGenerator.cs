using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SkEditor.SourceGenerators;

[Generator]
public class SettingsGenerator : IIncrementalGenerator
{
    private const string SettingsConfigAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.SettingsConfigAttribute";

    private const string ToggleSettingAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.ToggleSettingAttribute";

    private const string ChoiceSettingAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.ChoiceSettingAttribute";

    private const string SettingSymbolIconAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.SettingSymbolIconAttribute";

    private const string SettingPathIconAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.SettingPathIconAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol?>> classesWithAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                SettingsConfigAttributeFullName,
                (node, _) => node is ClassDeclarationSyntax,
                (symbolContext, _) => symbolContext.TargetSymbol as INamedTypeSymbol)
            .Where(symbol => symbol != null && symbol.DeclaringSyntaxReferences.Any(syntaxRef =>
            {
                SyntaxNode syntax = syntaxRef.GetSyntax();
                return syntax is ClassDeclarationSyntax classDecl &&
                       classDecl.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword));
            }))
            .Collect();

        var combined = classesWithAttribute.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, (ctx, tuple) =>
        {
            (ImmutableArray<INamedTypeSymbol?> classes, Compilation? compilation) = tuple;
            
            foreach (INamedTypeSymbol classSymbol in classes.Where(c => c is not null)!)
            {
                string className = classSymbol!.Name;
                string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                ImmutableArray<IPropertySymbol> properties =
                    [..classSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => p.SetMethod != null)];

                ImmutableArray<IPropertySymbol> toggleProperties =
                [
                    ..classSymbol.GetMembers().OfType<IPropertySymbol>().Where(p =>
                        p.GetAttributes().Any(attr =>
                            attr.AttributeClass?.ToDisplayString() == ToggleSettingAttributeFullName))
                ];

                ImmutableArray<IPropertySymbol> choiceProperties =
                [
                    ..classSymbol.GetMembers().OfType<IPropertySymbol>().Where(p =>
                        p.GetAttributes().Any(attr =>
                            attr.AttributeClass?.ToDisplayString() == ChoiceSettingAttributeFullName))
                ];

                StringBuilder settingsSectionsBuilder = new();
                foreach (IPropertySymbol toggleProperty in toggleProperties)
                {
                    AttributeData attr = toggleProperty.GetAttributes().First(a =>
                        a.AttributeClass?.ToDisplayString() == ToggleSettingAttributeFullName);

                    string icon = GetIconString(toggleProperty);

                    List<string> args = GetAttributeConstructorArgumentsFromSyntax(attr);

                    string titleKey = args.Count > 0 ? args[0] : "TranslationKey.None";
                    string categoryKey = args.Count > 1 ? args[1] : "TranslationKey.SettingsGeneralTitle";
                    string descriptionKey = args.Count > 2 ? args[2] : "TranslationKey.None";

                    settingsSectionsBuilder.AppendLine($"""
                                                        new ToggleSetting(
                                                            new TranslatableString({titleKey}),
                                                            {icon},
                                                            new TranslatableString({descriptionKey}),
                                                            () => {toggleProperty.Name},
                                                            (value) => {toggleProperty.Name} = value
                                                        ),
                                                        """);
                }

                foreach (IPropertySymbol choiceProperty in choiceProperties)
                {
                    AttributeData attr = choiceProperty.GetAttributes().First(a =>
                        a.AttributeClass?.ToDisplayString() == ChoiceSettingAttributeFullName);

                    List<string> args = GetAttributeConstructorArgumentsFromSyntax(attr);
                    string propertyType = choiceProperty.Type.ToDisplayString();
                    string icon = GetIconString(choiceProperty);

                    string titleKey = args.Count > 0 ? args[0] : "TranslationKey.None";
                    AttributeSyntax attributeSyntax = (AttributeSyntax)attr.ApplicationSyntaxReference!.GetSyntax();
                    string providerTypeName = GetFullyQualifiedTypeName(attributeSyntax, 1, compilation);
                    string categoryKey = args.Count > 2 ? args[2] : "TranslationKey.None";
                    string descriptionKey = args.Count > 3 ? args[3] : "TranslationKey.None";

                    settingsSectionsBuilder.AppendLine($"""
                                                        new ChoiceSetting<{propertyType}>(
                                                            new TranslatableString({titleKey}),
                                                            {icon},
                                                            new TranslatableString({descriptionKey}),
                                                            new {providerTypeName}(),
                                                            () => {choiceProperty.Name},
                                                            (value) => {choiceProperty.Name} = value
                                                        ),
                                                        """);
                }

                string rawCode = $$"""
                                   using System.Collections.Generic;
                                   using SkEditor.ViewModels.Pages;
                                   using SkEditor.Utilities.Localization;
                                   using SkEditor.Utilities.Settings.Models;
                                   using SkEditor.Utilities.Settings.Models.Types;

                                   namespace {{namespaceName}};
                                   public partial class {{className}}
                                   {
                                        protected override void CopyFrom({{className}} other)
                                        {
                                           if (other == null) return;
                                           {{string.Join("\n", properties.Select(p => $"{p.Name} = other.{p.Name};"))}}
                                        }
                                        
                                        public List<SettingElement> GetToggleSettingsSections()
                                        {
                                            return new List<SettingElement>
                                            {
                                                {{settingsSectionsBuilder}}
                                            };
                                        }
                                   }
                                   """;


                string formattedCode = CSharpSyntaxTree.ParseText(rawCode)
                    .GetRoot()
                    .NormalizeWhitespace()
                    .ToFullString();

                ctx.AddSource($"{className}_Generated.g.cs", formattedCode);
            }
        });
    }

    private static List<string> GetAttributeConstructorArgumentsFromSyntax(AttributeData attr)
    {
        if (attr.ApplicationSyntaxReference is null)
            return [];

        SyntaxNode syntaxNode = attr.ApplicationSyntaxReference.GetSyntax();
        if (syntaxNode is not AttributeSyntax attributeSyntax)
            return [];

        var argList = attributeSyntax.ArgumentList;
        if (argList is null)
            return [];

        return argList.Arguments
            .Select(a => a.Expression.ToString())
            .ToList();
    }

    private static string GetFullyQualifiedTypeName(AttributeSyntax attributeSyntax, int argumentIndex, Compilation compilation)
    {
        var argList = attributeSyntax.ArgumentList;
        if (argList is null || argList.Arguments.Count <= argumentIndex)
            return "object";

        var expression = argList.Arguments[argumentIndex].Expression;
        if (expression is not TypeOfExpressionSyntax typeOfExpr)
        {
            return expression.ToString().Replace("typeof(", "").TrimEnd(')');
        }

        SemanticModel model = compilation.GetSemanticModel(typeOfExpr.SyntaxTree);
        SymbolInfo symbolInfo = model.GetSymbolInfo(typeOfExpr.Type);
        if (symbolInfo.Symbol is INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return expression.ToString().Replace("typeof(", "").TrimEnd(')');
    }

    private static string GetIconString(IPropertySymbol toggleProperty)
    {
        AttributeData? symbolIconAttr = toggleProperty.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == SettingSymbolIconAttributeFullName);

        AttributeData? pathIconAttr = toggleProperty.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == SettingPathIconAttributeFullName);

        string? icon =
            "new FluentIcons.Avalonia.Fluent.SymbolIconSource() { Symbol = FluentIcons.Common.Symbol.QuestionCircle }";

        if (symbolIconAttr != null)
        {
            var iconArgs = symbolIconAttr.ConstructorArguments;
            string? symbol = iconArgs[0].Value?.ToString();
            icon =
                $"new FluentIcons.Avalonia.Fluent.SymbolIconSource() {{ Symbol = (FluentIcons.Common.Symbol){symbol} }}";
        }
        else if (pathIconAttr != null)
        {
            var iconArgs = pathIconAttr.ConstructorArguments;
            string? iconName = iconArgs[0].Value?.ToString();
            icon = $"SkEditor.Utilities.Icons.Icons.GetPathIcon(\"{iconName}\")";
        }

        return icon;
    }
}