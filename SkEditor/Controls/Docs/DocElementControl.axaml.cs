using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Syntax;

namespace SkEditor.Controls.Docs;

public partial class DocElementControl : UserControl
{
    public DocElementControl(IDocumentationEntry entry)
    {
        InitializeComponent();
        
        NameText.Text = entry.Name;
        Expander.Description = entry.DocType + " from " + entry.Addon;
        Expander.IconSource = new SymbolIconSource()
        {
            Symbol = GetSymbol(entry)
        };
        
        DescriptionText.Text = Format(string.IsNullOrEmpty(entry.Description) ? "No description provided." : entry.Description);
        VersionBadge.IconSource = new FontIconSource() { Glyph = "Since v" + (string.IsNullOrEmpty(entry.Version) ? "1.0.0" : entry.Version), };
        SourceBadge.IconSource = new FontIconSource() { Glyph = entry.Addon, };
        
        // Editor setup
        if (ApiVault.Get().GetAppConfig().Font.Equals("Default"))
        {
            Application.Current.TryGetResource("JetBrainsFont", Avalonia.Styling.ThemeVariant.Default, out var font);
            PatternsEditor.FontFamily = (FontFamily) font;
        }
        else
        {
            PatternsEditor.FontFamily = new FontFamily(ApiVault.Get().GetAppConfig().Font);
        }
        PatternsEditor.Text = Format(entry.Patterns);
        PatternsEditor.SyntaxHighlighting = CreatePatternHighlighting();
        PatternsEditor.TextArea.TextView.Redraw();
    }
    
    private Symbol GetSymbol(IDocumentationEntry entry)
    {
        return entry.DocType switch
        {
            IDocumentationEntry.Type.All => Symbol.WeatherRain,
            IDocumentationEntry.Type.Event => Symbol.Calendar,
            IDocumentationEntry.Type.Expression => Symbol.Calculator,
            IDocumentationEntry.Type.Effect => Symbol.Highlight,
            IDocumentationEntry.Type.Condition => Symbol.Filter,
            IDocumentationEntry.Type.Type => Symbol.Document,
            IDocumentationEntry.Type.Section => Symbol.Bookmark,
            IDocumentationEntry.Type.Structure => Symbol.Code,
            IDocumentationEntry.Type.Function => Symbol.Find,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string Format(string input)
    {
        return input.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&")
            .Replace("&quot;", "\"").Replace("&apos;", "'")
            .Replace("&#039;", "'").Replace("&#034;", "\"");
    }
    
    public IHighlightingDefinition CreatePatternHighlighting()
    {
        var highlighting = new EmptyHighlighting();
        var ruleSet = new HighlightingRuleSet();
        
        HighlightingSpan CreateSimpleCharRule(Regex pattern, Color color)
        {
            return new HighlightingSpan()
            {
                StartExpression = pattern,
                EndExpression = new Regex(""),
                RuleSet = ruleSet,
                StartColor = new HighlightingColor()
                {
                    Foreground = new SimpleHighlightingBrush(color)
                }
            };
        }
        
        HighlightingSpan CreateSurroundingRule(Regex start, Regex end, 
            Color delimiterColor, Color contentColor)
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
        
        ruleSet.Spans.Add(CreateSimpleCharRule(new Regex(@"\|"), Colors.Orange));
        ruleSet.Spans.Add(CreateSimpleCharRule(new Regex(@"\:"), Colors.Fuchsia));
        
        ruleSet.Spans.Add(CreateSurroundingRule(new Regex(@"\%"), new Regex(@"\%"), Colors.LimeGreen, Colors.Green));
        ruleSet.Spans.Add(CreateSurroundingRule(new Regex(@"\("), new Regex(@"\)"), Colors.Orange, Colors.OrangeRed));
        ruleSet.Spans.Add(CreateSurroundingRule(new Regex(@"\["), new Regex(@"\]"), Colors.Aqua, Colors.Teal));

        highlighting.MainRuleSet = ruleSet;
        return highlighting;
    }
    
    public class EmptyHighlighting : IHighlightingDefinition
    {
        public string Name => "Empty";
        public HighlightingRuleSet MainRuleSet { get; set; }
        public HighlightingRuleSet GetNamedRuleSet(string name) => new();
        public HighlightingColor GetNamedColor(string name) => new();
        public IEnumerable<HighlightingColor> NamedHighlightingColors => new List<HighlightingColor>();
        public IDictionary<string, string> Properties => new Dictionary<string, string>();
    }
}