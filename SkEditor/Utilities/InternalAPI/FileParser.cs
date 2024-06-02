using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using Octokit;
using SkEditor.API;
using SkEditor.Parser;
using SkEditor.Parser.Elements;
using SkEditor.TextMarkers;
using SkEditor.Views.Settings;
using Application = Avalonia.Application;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
/// Class that is attached to every opened TabViewItem it its
/// content is a TextEditor. Every parse stuff that uses the new
/// parser version (v2) should be done through this class!
/// </summary>
public class FileParser
{
    
    public readonly TextMarkerService TextMarkerService;
    public FileParser(TextEditor editor)
    {
        Editor = editor;
        
        /*
         * var textMarkerService = new TextMarkerService(textEditor.Document);
        textEditor.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
        textEditor.TextArea.TextView.LineTransformers.Add(textMarkerService);

        var services = textEditor.Document.GetService<IServiceContainer>();
        services?.AddService(typeof(ITextMarkerService), textMarkerService);

        TextMarkerServices[textEditor] = textMarkerService;
        return textMarkerService;
         */
        TextMarkerService = new TextMarkerService(Editor);
    }
    
    public TextEditor Editor { get; private set; }
    public bool IsParsed { get; private set; } = false;
    
    public List<Node> ParsedNodes { get; private set; }

    public void Parse()
    {
        var lines = Editor.Document.Lines
            .Select(line => Editor.Document.GetText(line));
        ParsedNodes = SectionParser.Parse(lines.ToArray());
        SkEditorAPI.Logs.Debug($"Parsed {ParsedNodes.Count} pure nodes, now parsing elements ...");

        var context = new ParsingContext();
        ElementParser.ParseNodes(ParsedNodes, context);
        SkEditorAPI.Logs.Debug($"Parsed {ParsedNodes.Count} nodes, with {context.Warnings.Count} warnings! [{context.ParsedNodes.Count}]");
        
        TextMarkerService.RemoveAll(marker => true);
        foreach (var pair in context.Warnings)
        {
            var node = pair.Item1;
            var warning = pair.Item2;
            var line = Editor.Document.GetLineByNumber(node.Line);
            
            var marker = TextMarkerService.Create(line.Offset, line.Length);
            marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
            marker.MarkerColor = Colors.Orange;
            marker.ToolTip = () => ParseContent(warning);
            SkEditorAPI.Logs.Debug("Adding marker to line " + node.Line);
        }
        SkEditorAPI.Logs.Debug("TextMarkerService.TextMarkers: " + TextMarkerService.TextMarkers.Count());

        /*void VisitNode(Node node, int indent = 0)
        {
            SkEditorAPI.Logs.Debug($"{new string(' ', indent)}- {node.Key} [{node.Element?.GetType().Name ?? "null"}]");
            if (node is SectionNode sectionNode)
            {
                foreach (var child in sectionNode.Children)
                {
                    VisitNode(child, indent + 2);
                }
            }
        }
        
        foreach (var node in ParsedNodes)
        {
            VisitNode(node);
        }*/
    }

    public StackPanel ParseContent(string input)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        
        var regex = new System.Text.RegularExpressions.Regex(@"<line:(\d+)>");
        var matches = regex.Matches(input);
        
        var lastMatch = 0;
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var index = match.Index;
            var length = match.Length;
            var line = int.Parse(match.Groups[1].Value);
            
            var text = input.Substring(lastMatch, index - lastMatch);
            var textBlock = new TextBlock { Text = text };
            panel.Children.Add(textBlock);
            
            var link = new TextBlock { Text = $"line {line}", Foreground = Brushes.CornflowerBlue };
            link.Tapped += (sender, args) =>
            {
                SkEditorAPI.Logs.Debug("Tapped on line " + line);
                Editor.ScrollToLine(line);
                Editor.ScrollToHorizontalOffset(0);
                Editor.CaretOffset = Editor.Document.GetLineByNumber(line).Offset;
                Editor.Focus();
            };
            panel.Children.Add(link);
            
            lastMatch = index + length;
        }
        
        return panel;
    }
}

public class ParserLineFormatter(int lineNumber, int length) : DocumentColorizingTransformer
{
    
    protected override void ColorizeLine(DocumentLine line)
    {
        if (!line.IsDeleted && line.LineNumber == lineNumber)
        {
            ChangeLinePart(line.Offset, line.EndOffset, ApplyChanges);
        }
    }

    private void ApplyChanges(VisualLineElement element)
    {
        Application.Current.TryGetResource("ThemeOrangeColor", ThemeVariant.Default, out var color);
        element. TextRunProperties. SetTextDecorations(new TextDecorationCollection(new[ ] {
            new TextDecoration {
                Location = TextDecorationLocation.Underline,
                StrokeDashArray = new AvaloniaList<double>(2, 2),
                Stroke = new SolidColorBrush((Color) color!),
                StrokeThickness = 2,
                StrokeThicknessUnit = TextDecorationUnit.Pixel,

                StrokeOffset = 3,
                StrokeOffsetUnit = TextDecorationUnit.Pixel }
        }));
    }
}