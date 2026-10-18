using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SkEditor.SourceGenerators;

[Generator]
public class SettingsGenerator : IIncrementalGenerator
{
    private const string SettingsConfigAttributeFullName =
        "SkEditor.Configuration.SettingsConfigAttribute";

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

        IncrementalValueProvider<(ImmutableArray<INamedTypeSymbol?> Left, Compilation Right)> combined =
            classesWithAttribute.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, (ctx, tuple) =>
        {
            (ImmutableArray<INamedTypeSymbol?> classes, Compilation? _) = tuple;

            foreach (INamedTypeSymbol classSymbol in classes.Where(c => c is not null)!)
            {
                string className = classSymbol!.Name;
                string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                ImmutableArray<IPropertySymbol> properties =
                    [..classSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => p.SetMethod != null)];

                bool generateCopyFrom = classSymbol.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == SettingsConfigAttributeFullName)
                    ?.ConstructorArguments.FirstOrDefault().Value as bool? ?? true;

                string copyFromMethod = generateCopyFrom
                    ? $$"""
                        protected override void CopyFrom({{className}} other)
                        {
                           if (other == null) return;
                           {{string.Join("\n", properties.Select(p => $"{p.Name} = other.{p.Name};"))}}
                        }
                        """
                    : "";

                string rawCode = $$"""
                                   #nullable enable

                                   using System;
                                   using System.Collections;
                                   using System.Collections.Generic;

                                   namespace {{namespaceName}};
                                   public partial class {{className}}
                                   {
                                        public enum {{className}}Property
                                        {
                                            {{string.Join(",\n", properties.Select(p => p.Name))}}
                                        }

                                        {{copyFromMethod}}

                                        public (Func<T> Get, Action<T> Set) GetProperty<T>({{className}}Property property)
                                        {
                                            return property switch
                                           {
                                               {{string.Join(",\n", properties.Select(p =>
                                                   $"{className}Property.{p.Name} => ((Func<T>)(object)(Func<{p.Type.ToDisplayString()}>)(() => {p.Name}), (Action<T>)(object)(Action<{p.Type.ToDisplayString()}>)(value => {p.Name} = ({p.Type.ToDisplayString()})(object{(p.NullableAnnotation == NullableAnnotation.Annotated ? "?" : "")})value))"
                                               ))}},
                                               _ => throw new ArgumentOutOfRangeException()
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