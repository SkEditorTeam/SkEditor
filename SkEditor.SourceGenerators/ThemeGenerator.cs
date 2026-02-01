using System.Collections.Immutable;
using System.Linq;
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
                        "Avalonia.Media.Immutable.ImmutableSolidColorBrush")
                ];

                string copyFromCode = $$"""
                                        using System;
                                        using Avalonia.Media.Immutable;

                                        namespace {{namespaceName}};
                                        public partial class {{className}}
                                        {
                                            public ImmutableSolidColorBrush GetBrushByName(string propertyName)
                                            {
                                                return propertyName switch
                                                {
                                                    {{string.Join("\n            ", properties.Select(p => $"\"{p.Name}\" => {p.Name},"))}}
                                                    _ => throw new ArgumentException($"Property {propertyName} not found in Theme")
                                                };
                                            }
                                            
                                            public void SetBrush(string propertyName, ImmutableSolidColorBrush brush)
                                            {
                                                switch (propertyName)
                                                {
                                                    {{string.Join("\n            ", properties.Select(p => $"""
                                                         case "{p.Name}":
                                                                        {p.Name} = brush;
                                                                        break;
                                                         """))}}
                                                    default:
                                                        throw new ArgumentException($"Property {propertyName} not found in Theme");
                                                }
                                            }
                                        }

                                        """;
                ctx.AddSource($"{className}_Bindings.g.cs", copyFromCode);
            }
        });
    }
}