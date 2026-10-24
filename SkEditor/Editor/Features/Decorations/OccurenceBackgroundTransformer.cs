using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.Appearance.Themes;

namespace SkEditor.Editor.Features.Decorations;

public class OccurenceBackgroundTransformer(IThemeService themes) : DocumentColorizingTransformer
{
    public int EndOffset { get; set; }
    public int LineNumber { get; set; }

    public int StartOffset { get; set; }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.LineNumber != LineNumber || StartOffset < 0 || EndOffset < 0)
        {
            return;
        }

        try
        {
            ChangeLinePart(StartOffset, EndOffset, ApplyChanges);
        }
        catch
        {
            // ignored
        }
    }

    private void ApplyChanges(VisualLineElement element)
    {
        Color editorBg = themes.CurrentTheme.EditorBackgroundColor.Color;
        Color selectionColor = themes.CurrentTheme.SelectionColor.Color;
        
        Color occurrenceColor = BlendColors(editorBg, selectionColor, 0.2);

        element.TextRunProperties.SetBackgroundBrush(new ImmutableSolidColorBrush(occurrenceColor));
    }

    private static Color BlendColors(Color baseColor, Color blend, double blendAmount)
    {
        byte r = (byte)((baseColor.R * (1 - blendAmount)) + (blend.R * blendAmount));
        byte g = (byte)((baseColor.G * (1 - blendAmount)) + (blend.G * blendAmount));
        byte b = (byte)((baseColor.B * (1 - blendAmount)) + (blend.B * blendAmount));
        byte a = (byte)((baseColor.A * (1 - blendAmount)) + (blend.A * blendAmount));

        return new Color(a, r, g, b);
    }
}