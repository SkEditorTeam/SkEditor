using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit.Rendering;
using SkEditor.Features;
using SkEditor.Views.Controls.Editor;

namespace SkEditor.ElementGenerators;

public class HexColorElementGenerator : VisualLineElementGenerator
{
    private static readonly Regex HexRegex = HexColorFeature.HexRegexGen();

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
}