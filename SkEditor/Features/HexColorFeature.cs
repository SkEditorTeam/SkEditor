using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public class HexColorFeature : IEditorFeature
{
    private readonly HexColorElementGenerator _colorGenerator = new();
    private readonly HexColorTransformer _colorTransformer = new();

    public bool CanAttach(FileTab tab)
    {
        return true;
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Add(_colorGenerator);
        editor.TextArea.TextView.LineTransformers.Add(_colorTransformer);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Remove(_colorGenerator);
        editor.TextArea.TextView.LineTransformers.Remove(_colorTransformer);
    }
}

public partial class HexColorElementGenerator : VisualLineElementGenerator
{
    private static readonly Regex HexRegex = HexRegexGen();

    public override int GetFirstInterestedOffset(int startOffset)
    {
        int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
        var document = CurrentContext.Document;
        string relevantText = document.GetText(startOffset, endOffset - startOffset);
        Match m = HexRegex.Match(relevantText);
        return m.Success ? startOffset + m.Index : -1;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
        int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
        string text = CurrentContext.Document.GetText(offset, endOffset - offset);
        Match m = HexRegex.Match(text);

        if (m is not { Success: true, Index: 0 })
        {
            return null!;
        }

        string pureHex = m.Groups["hex"].Value;

        if (!Color.TryParse($"#{pureHex}", out Color color))
        {
            return null!;
        }

        var control = new ColorBoxControl(color, CurrentContext.TextView, CurrentContext.GlobalTextRunProperties);

        TextBlock.SetBaselineOffset(control, CurrentContext.GlobalTextRunProperties.FontRenderingEmSize);
        control.VerticalAlignment = VerticalAlignment.Center;

        return new InlineObjectElement(0, control);
    }

    [GeneratedRegex(@"<#[#]?(?<hex>(?:[0-9a-fA-F]{3}){1,2})>")]
    private static partial Regex HexRegexGen();
}

public class ColorBoxControl(Color color, TextView textView, TextRunProperties properties) : Control
{
    private const int Gap = 1;
    private double ScaledGap => Gap * properties.FontRenderingEmSize / 16;

    protected override Size MeasureOverride(Size availableSize)
    {
        double height = textView.DefaultLineHeight;
        double boxSize = height * 0.75;

        return new Size(boxSize + ScaledGap, properties.FontRenderingEmSize);
    }

    public override void Render(DrawingContext context)
    {
        double height = Bounds.Height;
        double boxSize = height * 0.75;
        double yOffset = (height - boxSize) / 2;

        var rect = new Rect(0, yOffset, boxSize, boxSize);

        context.DrawRectangle(new SolidColorBrush(color), new Pen(Brushes.Gray, 0.5), rect);
    }
}

public partial class HexColorTransformer : DocumentColorizingTransformer
{
    private static readonly Regex HexRegex = HexRegexGen();

    protected override void ColorizeLine(DocumentLine line)
    {
        int lineStart = line.Offset;
        string text = CurrentContext.Document.GetText(line);

        foreach (Match m in HexRegex.Matches(text))
        {
            ChangeLinePart(
                lineStart + m.Index,
                lineStart + m.Index + m.Length,
                visualLine => { visualLine.TextRunProperties.SetForegroundBrush(Brushes.LightGray); });
        }
    }

    [GeneratedRegex(@"<#[#]?(?<hex>(?:[0-9a-fA-F]{3}){1,2})>")]
    private static partial Regex HexRegexGen();
}