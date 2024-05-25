using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Docs;
public partial class DocSyntaxColorizer
{
    public static IHighlightingDefinition CreatePatternHighlighting()
    {
        var highlighting = new EmptyHighlighting();
        var ruleSet = new HighlightingRuleSet();

        foreach (var span in CreateSyntaxRules(ruleSet))
            ruleSet.Spans.Add(span);

        highlighting.MainRuleSet = ruleSet;
        return highlighting;
    }

    private static List<HighlightingSpan> CreateSyntaxRules(HighlightingRuleSet ruleSet)
    {
        List<HighlightingSpan> spans = [];

        spans.Add(CreateSimpleCharRule(BarCharRegex(),
            Color.FromRgb(255, 204, 153), ruleSet));

        spans.Add(CreateSimpleCharRule(ColonCharRegex(),
            Color.FromRgb(204, 153, 255), ruleSet));

        spans.Add(CreateSurroundingRule(PercentCharRegex(), PercentCharRegex(),
            Color.FromRgb(153, 255, 204), Color.FromRgb(153, 204, 153), ruleSet));

        spans.Add(CreateSurroundingRule(OpeningBracketRegex(), ClosingBracketRegex(),
            Color.FromRgb(255, 255, 153), Color.FromRgb(255, 204, 102), ruleSet));

        spans.Add(CreateSurroundingRule(OpeningSquareBracketRegex(), ClosingSquareBracketRegex(),
            Color.FromRgb(204, 229, 255), Color.FromRgb(153, 204, 255), ruleSet));

        return spans;
    }

    private static HighlightingSpan CreateSimpleCharRule(Regex pattern, Color color, HighlightingRuleSet ruleSet)
    {
        return new HighlightingSpan()
        {
            StartExpression = pattern,
            EndExpression = EmptyRegex(),
            RuleSet = ruleSet,
            StartColor = new HighlightingColor()
            {
                Foreground = new SimpleHighlightingBrush(color)
            }
        };
    }

    private static HighlightingSpan CreateSurroundingRule(Regex start, Regex end,
        Color delimiterColor, Color contentColor, HighlightingRuleSet ruleSet)
    {
        return new HighlightingSpan()
        {
            StartExpression = start,
            EndExpression = end,
            RuleSet = ruleSet,
            StartColor = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(delimiterColor) },
            EndColor = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(delimiterColor) },
            SpanColor = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(contentColor) }
        };
    }

    public class EmptyHighlighting : IHighlightingDefinition
    {
        public string Name => "Empty";
        public HighlightingRuleSet MainRuleSet { get; set; }
        public HighlightingRuleSet GetNamedRuleSet(string name) => new();
        public HighlightingColor GetNamedColor(string name) => new();
        public IEnumerable<HighlightingColor> NamedHighlightingColors => [];
        public IDictionary<string, string> Properties => new Dictionary<string, string>();
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
}
