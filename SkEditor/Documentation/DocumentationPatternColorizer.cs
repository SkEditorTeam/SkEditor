using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace SkEditor.Utilities.Docs;

public partial class DocumentationPatternColorizer : DocumentColorizingTransformer
{
    private static readonly IBrush BarBrush = new SolidColorBrush(Color.FromRgb(255, 204, 153));
    private static readonly IBrush ColonBrush = new SolidColorBrush(Color.FromRgb(204, 153, 255));

    private static readonly IBrush PercentDelimiterBrush = new SolidColorBrush(Color.FromRgb(153, 255, 204));
    private static readonly IBrush PercentContentBrush = new SolidColorBrush(Color.FromRgb(153, 204, 153));

    private static readonly IBrush BracketDelimiterBrush = new SolidColorBrush(Color.FromRgb(255, 255, 153));
    private static readonly IBrush BracketContentBrush = new SolidColorBrush(Color.FromRgb(255, 204, 102));

    private static readonly IBrush SquareBracketDelimiterBrush = new SolidColorBrush(Color.FromRgb(204, 229, 255));
    private static readonly IBrush SquareBracketContentBrush = new SolidColorBrush(Color.FromRgb(153, 204, 255));

    protected override void ColorizeLine(DocumentLine line)
    {
        int lineStartOffset = line.Offset;
        string text = CurrentContext.Document.GetText(line);

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        ColorizeSurrounding(PercentRegex().Matches(text), lineStartOffset, PercentDelimiterBrush, PercentContentBrush);
        ColorizeSurrounding(BracketRegex().Matches(text), lineStartOffset, BracketDelimiterBrush, BracketContentBrush);
        ColorizeSurrounding(SquareBracketRegex().Matches(text), lineStartOffset, SquareBracketDelimiterBrush,
            SquareBracketContentBrush);

        ColorizeMatches(BarRegex().Matches(text), lineStartOffset, BarBrush);
        ColorizeMatches(ColonRegex().Matches(text), lineStartOffset, ColonBrush);
    }

    private void ColorizeMatches(MatchCollection matches, int lineStart, IBrush brush)
    {
        foreach (Match match in matches)
        {
            ChangeLinePart(
                lineStart + match.Index,
                lineStart + match.Index + match.Length,
                element => element.TextRunProperties.SetForegroundBrush(brush)
            );
        }
    }

    private void ColorizeSurrounding(MatchCollection matches, int lineStart, IBrush delimiterBrush, IBrush contentBrush)
    {
        foreach (Match match in matches)
        {
            if (match.Groups.Count != 4)
            {
                continue;
            }

            Group startDelim = match.Groups[1];
            Group content = match.Groups[2];
            Group endDelim = match.Groups[3];

            ChangeLinePart(lineStart + startDelim.Index, lineStart + startDelim.Index + startDelim.Length,
                e => e.TextRunProperties.SetForegroundBrush(delimiterBrush));

            if (content.Length > 0)
            {
                ChangeLinePart(lineStart + content.Index, lineStart + content.Index + content.Length,
                    e => e.TextRunProperties.SetForegroundBrush(contentBrush));
            }

            ChangeLinePart(lineStart + endDelim.Index, lineStart + endDelim.Index + endDelim.Length,
                e => e.TextRunProperties.SetForegroundBrush(delimiterBrush));
        }
    }

    [GeneratedRegex(@"\|")]
    private static partial Regex BarRegex();

    [GeneratedRegex(@"\:")]
    private static partial Regex ColonRegex();

    [GeneratedRegex(@"(\%)(.*?)(\%)")]
    private static partial Regex PercentRegex();

    [GeneratedRegex(@"(\()(.*?)(\))")]
    private static partial Regex BracketRegex();

    [GeneratedRegex(@"(\[)(.*?)(\])")]
    private static partial Regex SquareBracketRegex();
}