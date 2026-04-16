using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public class HexColorFeature : IEditorFeature
{
    private readonly HexColorElementGenerator _colorGenerator = new();

    public bool CanAttach(FileTab tab)
    {
        return true;
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Add(_colorGenerator);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Remove(_colorGenerator);
    }
}

public partial class HexColorElementGenerator : VisualLineElementGenerator
{
    private static readonly Regex HexRegex = HexMatcherRegex();

    private Match FindMatch(int startOffset)
    {
        int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
        var document = CurrentContext.Document;
        string relevantText = document.GetText(startOffset, endOffset - startOffset);
        return HexRegex.Match(relevantText);
    }

    public override int GetFirstInterestedOffset(int startOffset)
    {
        Match m = FindMatch(startOffset);
        return m.Success ? startOffset + m.Index : -1;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
        Match m = FindMatch(offset);
        if (m is not { Success: true, Index: 0 })
        {
            return null!;
        }

        string pureHex = m.Groups["hex"].Value;
        string prefix = m.Groups["prefix"].Value;

        if (!Color.TryParse($"#{pureHex}", out Color parsedColor))
        {
            return null!;
        }

        double lineHeight = CurrentContext.TextView.DefaultLineHeight;
        double size = lineHeight * 0.65;

        Border colorBox = new()
        {
            Background = new SolidColorBrush(parsedColor),
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(2),
            BorderBrush = Brushes.DimGray,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 3, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        TextBlock prefixText = new()
        {
            Text = prefix,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = CurrentContext.GlobalTextRunProperties.Typeface.FontFamily,
            FontSize = CurrentContext.GlobalTextRunProperties.FontRenderingEmSize,
            Foreground = CurrentContext.GlobalTextRunProperties.ForegroundBrush
        };

        StackPanel panel = new()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Height = lineHeight,
            ClipToBounds = true,
            Children = { prefixText, colorBox }
        };

        return new InlineObjectElement(prefix.Length, panel);
    }

    [GeneratedRegex(@"(?<prefix><#|&#|#)(?<hex>[A-Fa-f0-9]{6})>?")]
    private static partial Regex HexMatcherRegex();
}