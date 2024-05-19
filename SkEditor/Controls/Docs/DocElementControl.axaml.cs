using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Syntax;

namespace SkEditor.Controls.Docs;

public partial class DocElementControl : UserControl
{
    private bool _hasLoadedExamples = false;
    
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
        
        // Examples setup
        if (IDocProvider.Providers[entry.Provider].NeedsToLoadExamples)
        {
            _hasLoadedExamples = false;
            ExamplesEntry.IsExpanded = false;
            
            ExamplesEntry.Expanded += (_, _) =>
            {
                if (_hasLoadedExamples) 
                    return;
                _hasLoadedExamples = true;

                LoadExamples(entry);
            };
        }
        else
        {
            LoadExamples(entry);
        }
    }

    public async void LoadExamples(IDocumentationEntry entry)
    {
        var provider = IDocProvider.Providers[entry.Provider];
        
        // First we setup a small loading bar
        ExamplesEntry.Content = new ProgressBar()
        {
            IsIndeterminate = true,
            Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(5)
        };
        
        // Then we load the examples
        try
        {
            var examples = await provider.FetchExamples(entry.Id);
            ExamplesEntry.Content = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
            };

            foreach (IDocumentationExample example in examples)
            {
                var stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 2)
                };
                
                stackPanel.Children.Add(new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        new TextBlock()
                        {
                            Text = "By " + example.Author,
                            FontWeight = FontWeight.Regular,
                            FontSize = 16
                        },
                        new InfoBadge() { IconSource = new FontIconSource() { Glyph = example.Votes } }
                    }
                });

                object GetAppResource(string key)
                {
                    Application.Current.TryGetResource(key, Avalonia.Styling.ThemeVariant.Default, out var resource);
                    return resource;
                }

                var textEditor = new TextEditor()
                {
                    Foreground = (IBrush)GetAppResource("EditorTextColor"),
                    Background = (IBrush)GetAppResource("EditorBackgroundColor"),
                    Padding = new Thickness(10),
                    Text = Format(example.Example),
                    IsReadOnly = true,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                
                if (ApiVault.Get().GetAppConfig().Font.Equals("Default"))
                {
                    Application.Current.TryGetResource("JetBrainsFont", Avalonia.Styling.ThemeVariant.Default, out var font);
                    textEditor.FontFamily = (FontFamily) font;
                }
                else
                {
                    textEditor.FontFamily = new FontFamily(ApiVault.Get().GetAppConfig().Font);
                }
                
                textEditor.SyntaxHighlighting = SyntaxLoader.GetCurrentSkirptHighlighting();
                stackPanel.Children.Add(textEditor);
                
                ((StackPanel) ExamplesEntry.Content).Children.Add(stackPanel);
            }
        }
        catch (Exception e)
        {
            ExamplesEntry.Content = new TextBlock()
            {
                Text = "An error occurred while loading examples: " + e.Message,
                Foreground = Brushes.Red,
                FontWeight = FontWeight.SemiLight,
                TextWrapping = TextWrapping.Wrap
            };
        }
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
            IDocumentationEntry.Type.Type => Symbol.Library,
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