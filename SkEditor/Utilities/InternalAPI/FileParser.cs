using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Octokit;
using SkEditor.API;
using SkEditor.Parser;
using SkEditor.Parser.Elements;
using SkEditor.Utilities.Editor.TextMarkers;
using SkEditor.Utilities.Styling;
using Application = Avalonia.Application;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
/// Class that is attached to every opened TabViewItem it its
/// content is a TextEditor. Every parse stuff that uses the new
/// parser version (v2) should be done through this class!
/// </summary>
public class FileParser
{
    
    public FileParser(TextEditor editor)
    {
        Editor = editor;
    }
    
    public TextEditor Editor { get; private set; }
    public bool IsParsed { get; private set; } = false;
    
    public List<Node> ParsedNodes { get; private set; }

    public void Parse()
    {
        var lines = Editor.Document.Lines
            .Select(line => Editor.Document.GetText(line));
        ParsedNodes = SectionParser.Parse(lines.ToArray());
        SkEditorAPI.Logs.Debug($"Parsed {ParsedNodes.Count} nodes");

        var context = new ParsingContext();
        ElementParser.ParseNodes(ParsedNodes, context);
        SkEditorAPI.Logs.Debug($"Parsed {ParsedNodes.Count} nodes, with {context.Warnings.Count} warnings");
        
        foreach (var pair in context.Warnings)
        {
            var node = pair.Item1;
            var warning = pair.Item2;
            
            // add an underline to the line
            var line = Editor.Document.GetLineByNumber(node.Line);
            Editor.TextArea.TextView.LineTransformers
                .Add(new LineFormatter(node.Line, line.Length));
        }
        
    }
}

public class LineFormatter(int lineNumber, int length) : DocumentColorizingTransformer
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
        element.BackgroundBrush = new SolidColorBrush((Color) color);
    }
}