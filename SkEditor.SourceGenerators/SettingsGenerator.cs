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
        "SkEditor.Utilities.Settings.Attributes.SettingsConfigAttribute";

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
                                   using System;
                                   using System.Collections;
                                   using System.Collections.Generic;
                                   using SkEditor.ViewModels.Pages;
                                   using SkEditor.Utilities.Localization;
                                   using SkEditor.Utilities.Settings.Models;
                                   using SkEditor.Utilities.Settings.Models.Types;

                                   namespace {{namespaceName}};
                                   public partial class {{className}}
                                   {
                                        private readonly Dictionary<string, object> _getters;
                                        private readonly Dictionary<string, object> _setters;

                                        public {{className}}()
                                        {
                                            _getters = new()
                                            {
                                                {{string.Join(",\n", properties.Select(p => $"[\"{p.Name}\"] = (Func<{p.Type}>)(() => {p.Name})"))}}
                                            };
                                            _setters = new()
                                            {
                                                {{string.Join(",\n", properties.Select(p => $"[\"{p.Name}\"] = (Action<{p.Type}>)(v => {p.Name} = v)"))}}
                                            };
                                        }

                                        {{copyFromMethod}}

                                        public Func<T> GetGetter<T>(string property) =>
                                            _getters.TryGetValue(property, out var v) ? (Func<T>)v : null;

                                        public Action<T> GetSetter<T>(string property) =>
                                            _setters.TryGetValue(property, out var v) ? (Action<T>)v : null;
                                            
                                        public (Func<T> Get, Action<T> Set) GetProperty<T>(string property) =>
                                            (GetGetter<T>(property), GetSetter<T>(property));
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