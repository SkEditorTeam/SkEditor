using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SkEditor.SourceGenerators;

[Generator]
public class ThemeGenerator : IIncrementalGenerator
{
    private const string ThemeAttributeFullName =
        "SkEditor.Utilities.Styling.Attributes.ThemeAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol?>> classesWithAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ThemeAttributeFullName,
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
                [
                    ..classSymbol.GetMembers().OfType<IPropertySymbol>().Where(p =>
                        p.SetMethod != null && p.Type.ToDisplayString() ==
                        "Avalonia.Media.Immutable.ImmutableSolidColorBrush" && p.GetAttributes().Any(attr =>
                            attr.AttributeClass?.ToDisplayString() ==
                            "SkEditor.Utilities.Styling.Attributes.ThemeResourceAttribute"))
                ];

                StringBuilder getBrushSwitchArms = new();
                foreach (IPropertySymbol? prop in properties)
                {
                    getBrushSwitchArms.AppendLine($"\"{prop.Name}\" => {prop.Name},");
                }

                StringBuilder setBrushCases = new();
                foreach (IPropertySymbol? prop in properties)
                {
                    setBrushCases.AppendLine($"""
                                                  case "{prop.Name}":
                                                      {prop.Name} = brush;
                                                      break;
                                              """);
                }

                StringBuilder themeToResourceDictEntries = new();
                foreach (IPropertySymbol? prop in properties)
                {
                    AttributeData attr = prop.GetAttributes().First(a =>
                        a.AttributeClass?.ToDisplayString() ==
                        "SkEditor.Utilities.Styling.Attributes.ThemeResourceAttribute");

                    List<string> keys = attr.ConstructorArguments.Length > 0
                        ? attr.ConstructorArguments[0].Values.Select(v => v.Value?.ToString() ?? "").ToList()
                        : [];

                    bool mapPropertyName = true;
                    KeyValuePair<string, TypedConstant> namedArg =
                        attr.NamedArguments.FirstOrDefault(kv => kv.Key == "MapPropertyName");

                    if (namedArg.Key is not null && namedArg.Value.Value is bool explicitValue)
                    {
                        mapPropertyName = explicitValue;
                    }

                    if (mapPropertyName)
                    {
                        keys.Add(prop.Name);
                    }

                    IEnumerable<string> finalKeys = keys.Distinct();

                    themeToResourceDictEntries.AppendLine(
                        $"{{ \"{prop.Name}\", [ {string.Join(", ", finalKeys.Select(k => $"\"{k}\""))} ] }},");
                }

                string rawCode = $$"""
                                   using System;
                                   using System.Collections.Generic;
                                   using Avalonia.Media.Immutable;

                                   namespace {{namespaceName}};

                                   public partial class {{className}}
                                   {
                                       public ImmutableSolidColorBrush GetBrushByName(string propertyName)
                                       {
                                           return propertyName switch
                                           {
                                   {{getBrushSwitchArms}}
                                               _ => throw new ArgumentException($"Property {propertyName} not found in Theme")
                                           };
                                       }
                                       
                                       public void SetBrush(string propertyName, ImmutableSolidColorBrush brush)
                                       {
                                           switch (propertyName)
                                           {
                                   {{setBrushCases}}
                                               default:
                                                   throw new ArgumentException($"Property {propertyName} not found in Theme");
                                           }
                                       }
                                       
                                       public static readonly Dictionary<string, string[]> ThemeToResourceDictionary = new()
                                       {
                                       {{themeToResourceDictEntries}}
                                       };
                                   }
                                   """;

                string formattedCode = CSharpSyntaxTree.ParseText(rawCode)
                    .GetRoot()
                    .NormalizeWhitespace()
                    .ToFullString();

                ctx.AddSource($"{className}_Bindings.g.cs", formattedCode);
            }
        });
    }
}