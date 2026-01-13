using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace SkEditor.SourceGenerators;

[Generator]
public class SettingsGenerator : IIncrementalGenerator
{
    private const string SettingsConfigAttributeFullName = "SkEditor.Utilities.Settings.Attributes.SettingsConfigAttribute";
    
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
                return syntax is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword));
            }))
            .Collect();

        context.RegisterSourceOutput(classesWithAttribute, (ctx, classes) =>
        {
            foreach (INamedTypeSymbol classSymbol in classes.Where(c => c is not null)!)
            {
                string className = classSymbol!.Name;
                string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                ImmutableArray<IPropertySymbol> properties = [..classSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => p.SetMethod != null)];
                
                var copyFromCode = $$"""
                                     namespace {{namespaceName}}
                                     {
                                         public partial class {{className}}
                                         {
                                             protected override void CopyFrom({{className}} other)
                                             {
                                                 if (other == null) return;
                                                 {{string.Join("\n            ", properties.Select(p => $"{p.Name} = other.{p.Name};"))}}
                                             }
                                         }
                                     }

                                     """;
                ctx.AddSource($"{className}_CopyFrom.g.cs", copyFromCode);
            }
        });
    }
}