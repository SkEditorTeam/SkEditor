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

    private const string SettingSymbolIconAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.SettingSymbolIconAttribute";

    private const string SettingPathIconAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.SettingPathIconAttribute";

    private const string SettingCategoryAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.SettingCategoryAttribute";

    private const string CategoryMetadataAttributeFullName =
        "SkEditor.Utilities.Settings.Attributes.CategoryMetadataAttribute";

    private const string SettingsCategoryEnumFullName =
        "SkEditor.Utilities.Settings.Models.SettingsCategory";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol?>> classesWithAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                SettingsConfigAttributeFullName,
                (node, _) => node is ClassDeclarationSyntax,
                (symbolContext, _) => symbolContext.TargetSymbol as INamedTypeSymbol)
            .Where(s => s != null).Collect();

        IncrementalValueProvider<INamedTypeSymbol?> categoryEnum = context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => node is EnumDeclarationSyntax { Identifier.Text: "SettingsCategory" },
                (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
            .Where(s => s != null).Collect().Select((arr, _) =>
                arr.FirstOrDefault(s => s!.ToDisplayString() == SettingsCategoryEnumFullName));

        IncrementalValueProvider<((ImmutableArray<INamedTypeSymbol?> Left, Compilation Right) Left, INamedTypeSymbol?
            Right)> combined = classesWithAttribute.Combine(context.CompilationProvider).Combine(categoryEnum);

        context.RegisterSourceOutput(combined, (ctx, tuple) =>
        {
            ImmutableArray<INamedTypeSymbol?> classes = tuple.Left.Left;
            Compilation compilation = tuple.Left.Right;
            INamedTypeSymbol? enumSymbol = tuple.Right;

            if (enumSymbol == null)
            {
                return;
            }

            Dictionary<string, CategoryMeta> categoryData = ParseEnumCategories(enumSymbol);

            foreach (INamedTypeSymbol classSymbol in classes.Where(c => c is not null)!)
            {
                string className = classSymbol!.Name;
                string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

                HashSet<string> usings = [];
                foreach (SyntaxReference? syntaxRef in classSymbol.DeclaringSyntaxReferences)
                {
                    CompilationUnitSyntax root = syntaxRef.GetSyntax().SyntaxTree.GetCompilationUnitRoot();
                    foreach (UsingDirectiveSyntax u in root.Usings)
                    {
                        usings.Add(u.ToString());
                    }
                }

                List<IPropertySymbol> properties = classSymbol.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => p.SetMethod != null).ToList();

                Dictionary<string, List<string>> categorySettings = new();

                foreach (IPropertySymbol property in properties)
                {
                    (string Category, string Code)? processed = ProcessProperty(property, compilation);
                    if (!processed.HasValue)
                    {
                        continue;
                    }

                    if (!categorySettings.ContainsKey(processed.Value.Category))
                    {
                        categorySettings[processed.Value.Category] = new List<string>();
                    }

                    categorySettings[processed.Value.Category].Add(processed.Value.Code);
                }

                string treeBuilderCode = GenerateTreeBuilder(categoryData, categorySettings);

                string rawCode = $$"""
                                   {{string.Join("\n", usings)}}
                                   using System.Collections.Generic;
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
                                        
                                        public List<SettingSection> GetSettingsTree()
                                        {
                                            {{treeBuilderCode}}
                                        }
                                   }
                                   """;

                string formattedCode =
                    CSharpSyntaxTree.ParseText(rawCode).GetRoot().NormalizeWhitespace().ToFullString();
                ctx.AddSource($"{className}_Generated.g.cs", formattedCode);
            }
        });
    }

    private static Dictionary<string, CategoryMeta> ParseEnumCategories(INamedTypeSymbol enumSymbol)
    {
        Dictionary<string, CategoryMeta> result = new();

        foreach (IFieldSymbol? member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            AttributeData? attr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == CategoryMetadataAttributeFullName);

            if (attr == null)
            {
                continue;
            }

            string id = member.Name;
            string titleKey = "SkEditor.Utilities.Localization.TranslationKey.None";
            string symbol = "FluentIcons.Common.Symbol.Settings";
            string parentId = "null";

            if (attr.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax { ArgumentList: not null } syntax)
            {
                SeparatedSyntaxList<AttributeArgumentSyntax> args = syntax.ArgumentList.Arguments;

                if (args.Count > 0 && args[0].NameEquals == null)
                {
                    titleKey = args[0].Expression.ToString();
                }

                if (args.Count > 1 && args[1].NameEquals == null)
                {
                    symbol = args[1].Expression.ToString();
                }

                AttributeArgumentSyntax? parentArg =
                    args.FirstOrDefault(na => na.NameEquals?.Name.Identifier.Text == "Parent");
                if (parentArg != null)
                {
                    parentId = parentArg.Expression.ToString().Split('.').Last();
                }
            }

            result[id] = new CategoryMeta { Id = id, TitleKey = titleKey, Symbol = symbol, ParentId = parentId };
        }

        return result;
    }

    private static string GenerateTreeBuilder(Dictionary<string, CategoryMeta> categories,
        Dictionary<string, List<string>> settings)
    {
        HashSet<string> usedCategories = [];

        foreach (CategoryMeta cat in categories.Values)
        {
            if (!settings.ContainsKey(cat.Id))
            {
                continue;
            }

            usedCategories.Add(cat.Id);

            string currentParent = cat.ParentId;
            while (currentParent != "null" && categories.ContainsKey(currentParent))
            {
                usedCategories.Add(currentParent);
                currentParent = categories[currentParent].ParentId;
            }
        }

        if (usedCategories.Count == 0)
        {
            return "return new System.Collections.Generic.List<SettingSection>();";
        }

        StringBuilder sb = new();

        foreach (string catId in usedCategories)
        {
            CategoryMeta cat = categories[catId];
            sb.AppendLine(
                $"var cat_{cat.Id} = new SettingSection(new TranslatableString({cat.TitleKey}), new FluentIcons.Avalonia.Fluent.SymbolIconSource {{ Symbol = (FluentIcons.Common.Symbol){cat.Symbol} }}, []);");

            if (!settings.TryGetValue(cat.Id, out List<string>? catSettings))
            {
                continue;
            }

            foreach (string? settingCode in catSettings)
            {
                sb.AppendLine($"cat_{cat.Id}.Settings.Add({settingCode});");
            }
        }

        sb.AppendLine();
        sb.AppendLine("var roots = new System.Collections.Generic.List<SettingSection>();");

        foreach (string catId in usedCategories)
        {
            CategoryMeta cat = categories[catId];
            if (cat.ParentId != "null" && usedCategories.Contains(cat.ParentId))
            {
                sb.AppendLine($"cat_{cat.ParentId}.Subsections.Add(cat_{cat.Id});");
            }
            else
            {
                sb.AppendLine($"roots.Add(cat_{cat.Id});");
            }
        }

        sb.AppendLine("return roots;");
        return sb.ToString();
    }

    private static (string Category, string Code)? ProcessProperty(IPropertySymbol property, Compilation compilation)
    {
        AttributeData? settingAttr = property.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name.EndsWith("SettingAttribute") == true);
        if (settingAttr == null)
        {
            return null;
        }

        string attrName = settingAttr.AttributeClass!.Name;
        string iconCode = GetIconString(property);
        string category = GetCategory(property);

        string? generatedCode = attrName switch
        {
            "ToggleSettingAttribute" => GenerateToggle(property, settingAttr, iconCode),
            "ChoiceSettingAttribute" => GenerateChoice(property, settingAttr, iconCode, compilation),
            _ => null
        };

        if (generatedCode == null)
        {
            return null;
        }

        return (category, generatedCode);
    }

    private static string GetCategory(IPropertySymbol property)
    {
        AttributeData? categoryAttr = property.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == SettingCategoryAttributeFullName);

        if (categoryAttr == null)
        {
            return "General";
        }

        List<string> syntaxArgs = GetArgsFromSyntax(categoryAttr);
        if (syntaxArgs.Count > 0)
        {
            return syntaxArgs[0].Split('.').Last();
        }

        return categoryAttr.ConstructorArguments.Length > 0
            ? categoryAttr.ConstructorArguments[0].ToCSharpString().Split('.').Last()
            : "General";
    }

    private static string GenerateToggle(IPropertySymbol property, AttributeData attr, string icon)
    {
        List<string> syntaxArgs = GetArgsFromSyntax(attr);

        string titleKey = syntaxArgs.Count > 0 ? syntaxArgs[0] : "SkEditor.Utilities.Localization.TranslationKey.None";
        string descriptionKey =
            syntaxArgs.Count > 1 ? syntaxArgs[1] : "SkEditor.Utilities.Localization.TranslationKey.None";

        string descriptionPart = descriptionKey.Contains("TranslationKey.None")
            ? "null"
            : $"new SkEditor.Utilities.Localization.TranslatableString({descriptionKey})";

        return $"""
                new SkEditor.Utilities.Settings.Models.Types.ToggleSetting(
                    new SkEditor.Utilities.Localization.TranslatableString({titleKey}),
                    {icon},
                    {descriptionPart},
                    () => {property.Name},
                    (value) => {property.Name} = value
                )
                """;
    }

    private static string GenerateChoice(IPropertySymbol property, AttributeData attr, string icon,
        Compilation compilation)
    {
        List<string> syntaxArgs = GetArgsFromSyntax(attr);

        string titleKey = syntaxArgs.Count > 0 ? syntaxArgs[0] : "SkEditor.Utilities.Localization.TranslationKey.None";
        string descriptionKey =
            syntaxArgs.Count > 2 ? syntaxArgs[2] : "SkEditor.Utilities.Localization.TranslationKey.None";

        string descriptionPart = descriptionKey.Contains("TranslationKey.None")
            ? "null"
            : $"new SkEditor.Utilities.Localization.TranslatableString({descriptionKey})";

        string providerCode = "null";

        if (attr.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax
            {
                ArgumentList.Arguments.Count: > 1
            } syntax)
        {
            ExpressionSyntax expr = syntax.ArgumentList.Arguments[1].Expression;
            if (expr is TypeOfExpressionSyntax typeOfExpr)
            {
                SemanticModel model = compilation.GetSemanticModel(typeOfExpr.SyntaxTree);

                if (model.GetSymbolInfo(typeOfExpr.Type).Symbol is INamedTypeSymbol typeSymbol)
                {
                    string typeFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    IMethodSymbol? ctor = typeSymbol.Constructors
                        .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                        .OrderByDescending(c => c.Parameters.Length)
                        .FirstOrDefault();

                    if (ctor == null || ctor.Parameters.Length == 0)
                    {
                        providerCode = $"new {typeFullName}()";
                    }
                    else
                    {
                        IEnumerable<string> ctorArgs = ctor.Parameters.Select(p =>
                            $"Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(SkEditor.App.Services)");

                        providerCode = $"new {typeFullName}({string.Join(", ", ctorArgs)})";
                    }
                }
                else
                {
                    string fallbackType = typeOfExpr.Type.ToString();
                    providerCode = $"new {fallbackType}()";
                }
            }
            else
            {
                string fallbackType = expr.ToString().Replace("typeof(", "").TrimEnd(')');
                providerCode = $"new {fallbackType}()";
            }
        }

        string propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return $"""
                new SkEditor.Utilities.Settings.Models.Types.ChoiceSetting<{propertyType}>(
                    new SkEditor.Utilities.Localization.TranslatableString({titleKey}),
                    {icon},
                    {descriptionPart},
                    {providerCode},
                    () => {property.Name},
                    (value) => {property.Name} = value
                )
                """;
    }

    private static List<string> GetArgsFromSyntax(AttributeData attr)
    {
        if (attr.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax { ArgumentList: not null } syntax)
        {
            return syntax.ArgumentList.Arguments.Select(a => a.Expression.ToString()).ToList();
        }

        return [];
    }

    private static string GetIconString(IPropertySymbol prop)
    {
        AttributeData? symAttr = prop.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == SettingSymbolIconAttributeFullName);
        AttributeData? pathAttr = prop.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == SettingPathIconAttributeFullName);

        if (symAttr != null)
        {
            string symbolVal = symAttr.ConstructorArguments[0].ToCSharpString();
            return
                $"new FluentIcons.Avalonia.Fluent.SymbolIconSource() {{ Symbol = (FluentIcons.Common.Symbol){symbolVal} }}";
        }

        if (pathAttr == null)
        {
            return
                "new FluentIcons.Avalonia.Fluent.SymbolIconSource() { Symbol = FluentIcons.Common.Symbol.QuestionCircle }";
        }

        List<string> syntaxArgs = GetArgsFromSyntax(pathAttr);
        string pathVal = syntaxArgs.Count > 0 ? syntaxArgs[0] : pathAttr.ConstructorArguments[0].ToCSharpString();
        return $"SkEditor.Utilities.Icons.Icons.GetPathIcon({pathVal})";
    }

    private class CategoryMeta
    {
        public string Id { get; set; } = null!;
        public string TitleKey { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public string ParentId { get; set; } = null!;
    }
}