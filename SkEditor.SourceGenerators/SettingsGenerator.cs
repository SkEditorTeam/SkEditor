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

        context.RegisterSourceOutput(classesWithAttribute, (ctx, classes) =>
        {
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

                StringBuilder settingsSectionsBuilder = new();
                foreach (IPropertySymbol toggleProperty in toggleProperties)
                {
                    AttributeData attr = toggleProperty.GetAttributes().First(a =>
                        a.AttributeClass?.ToDisplayString() == ToggleSettingAttributeFullName);

                    var args = attr.ConstructorArguments;

                     string? titleKey = args[0].Value?.ToString();
                     string? icon = args[1].Value?.ToString();
                     string? categoryKey = args.Length > 2 ? args[2].Value?.ToString() : "SettingsGeneralTitle";
                     string? descriptionKey = args.Length > 3 ? args[3].Value?.ToString() : null;

                     settingsSectionsBuilder.AppendLine($$"""
                                                          new ToggleSetting(
                                                              Translation.GetObservable("{{titleKey}}"),
                                                              new FluentIcons.Avalonia.Fluent.SymbolIconSource() { Symbol = (FluentIcons.Common.Symbol){{icon}} },
                                                              {{(descriptionKey != null ? $"Translation.GetObservable(\"{descriptionKey}\")" : "null")}},
                                                              () => {{toggleProperty.Name}},
                                                              (value) => {{toggleProperty.Name}} = value
                                                          ),
                                                          """);
                }

                string rawCode = $$"""
                                   using System.Collections.Generic;
                                   using SkEditor.Utilities.Localization;
                                   using SkEditor.ViewModels.Pages;
                                   
                                   namespace {{namespaceName}};
                                   public partial class {{className}}
                                   {
                                        protected override void CopyFrom({{className}} other)
                                        {
                                           if (other == null) return;
                                           {{string.Join("\n", properties.Select(p => $"{p.Name} = other.{p.Name};"))}}
                                        }
                                        
                                        public List<ToggleSetting> GetToggleSettingsSections()
                                        {
                                            return new List<ToggleSetting>
                                            {
                                                {{settingsSectionsBuilder.ToString()}}
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
}