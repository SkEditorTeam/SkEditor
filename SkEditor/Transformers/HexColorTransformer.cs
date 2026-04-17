using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.Features;

namespace SkEditor.Transformers;

public class HexColorTransformer : DocumentColorizingTransformer
{
    private static readonly Regex HexRegex = HexColorFeature.HexRegexGen();

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
}