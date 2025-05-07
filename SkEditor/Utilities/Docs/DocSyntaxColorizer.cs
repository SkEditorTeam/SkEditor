using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;

namespace SkEditor.Utilities.Docs;

public partial class DocSyntaxColorizer
{
    public static IHighlightingDefinition CreatePatternHighlighting()
    {
        HighlightingRuleSet ruleSet = new();

        foreach (HighlightingSpan span in CreateSyntaxRules(ruleSet))
        {
            ruleSet.Spans.Add(span);
        }

        EmptyHighlighting highlighting = new()
        {
            MainRuleSet = ruleSet,
        };

        return highlighting;
    }

    private static List<HighlightingSpan> CreateSyntaxRules(HighlightingRuleSet ruleSet)
    {
        List<HighlightingSpan> spans =
        [
            CreateSimpleCharRule(BarCharRegex(),
                Color.FromRgb(255, 204, 153), ruleSet),

            CreateSimpleCharRule(ColonCharRegex(),
                Color.FromRgb(204, 153, 255), ruleSet),

            CreateSurroundingRule(PercentCharRegex(), PercentCharRegex(),
                Color.FromRgb(153, 255, 204), Color.FromRgb(153, 204, 153), ruleSet),

            CreateSurroundingRule(OpeningBracketRegex(), ClosingBracketRegex(),
                Color.FromRgb(255, 255, 153), Color.FromRgb(255, 204, 102), ruleSet),

            CreateSurroundingRule(OpeningSquareBracketRegex(), ClosingSquareBracketRegex(),
                Color.FromRgb(204, 229, 255), Color.FromRgb(153, 204, 255), ruleSet)
        ];

        return spans;
    }

    private static HighlightingSpan CreateSimpleCharRule(Regex pattern, Color color, HighlightingRuleSet ruleSet)
    {
        return new HighlightingSpan
        {
            StartExpression = pattern,
            EndExpression = EmptyRegex(),
            RuleSet = ruleSet,
            StartColor = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(color)
            }
        };
    }

    private static HighlightingSpan CreateSurroundingRule(Regex start, Regex end,
        Color delimiterColor, Color contentColor, HighlightingRuleSet ruleSet)
    {
        return new HighlightingSpan
        {
            StartExpression = start,
            EndExpression = end,
            RuleSet = ruleSet,
            StartColor = new HighlightingColor { Foreground = new SimpleHighlightingBrush(delimiterColor) },
            EndColor = new HighlightingColor { Foreground = new SimpleHighlightingBrush(delimiterColor) },
            SpanColor = new HighlightingColor { Foreground = new SimpleHighlightingBrush(contentColor) }
        };
    }

    [GeneratedRegex("")]
    private static partial Regex EmptyRegex();

    [GeneratedRegex(@"\|")]
    private static partial Regex BarCharRegex();

    [GeneratedRegex(@"\:")]
    private static partial Regex ColonCharRegex();

    [GeneratedRegex(@"\%")]
    private static partial Regex PercentCharRegex();

    [GeneratedRegex(@"\(")]
    private static partial Regex OpeningBracketRegex();

    [GeneratedRegex(@"\)")]
    private static partial Regex ClosingBracketRegex();

    [GeneratedRegex(@"\[")]
    private static partial Regex OpeningSquareBracketRegex();

    [GeneratedRegex(@"\]")]
    private static partial Regex ClosingSquareBracketRegex();

    public class EmptyHighlighting : IHighlightingDefinition
    {
        public string Name => "Empty";
        public required HighlightingRuleSet MainRuleSet { get; set; }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            return new HighlightingRuleSet();
        }

        public HighlightingColor GetNamedColor(string name)
        {
            return new HighlightingColor();
        }

        public IEnumerable<HighlightingColor> NamedHighlightingColors => [];
        public IDictionary<string, string> Properties => new Dictionary<string, string>();
    }
}