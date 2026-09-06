using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AvaloniaEdit.Document;
using SkEditor.Models.Completions;

namespace SkEditor.Utilities.Editor.Completions;

public partial class VariableCompletionProvider : ICompletionProvider
{
    public bool CheckCompletionsAvailable(TextDocument document, CompletionContext context)
    {
        if (context.Word.StartsWith('{'))
        {
            return true;
        }

        if (context.StartOffset <= 0)
        {
            return false;
        }

        char previousChar = document.GetCharAt(context.StartOffset - 1);
        return previousChar == '{';
    }

    public void PopulateCompletions(TextDocument document, CompletionContext context, List<CompletionData> completions)
    {
        HashSet<string> seenVariables = new(StringComparer.OrdinalIgnoreCase);
        DocumentLine currentLine = document.GetLineByOffset(context.StartOffset);

        int blockStartOffset = 0;
        DocumentLine? prev = currentLine.PreviousLine;
        while (prev != null)
        {
            if (prev.Length > 0 && !char.IsWhiteSpace(document.GetCharAt(prev.Offset)) &&
                document.GetCharAt(prev.Offset) != '#')
            {
                blockStartOffset = prev.Offset;
                break;
            }

            prev = prev.PreviousLine;
        }

        string fullText = document.Text;

        foreach (ValueMatch match in VariableRegex().EnumerateMatches(fullText.AsSpan()))
        {
            int matchStart = match.Index;
            int matchEnd = match.Index + match.Length;

            if (matchStart <= context.StartOffset && matchEnd >= context.StartOffset)
            {
                continue;
            }

            ReadOnlySpan<char> matchSpan = fullText.AsSpan(matchStart, match.Length);

            if (!matchSpan.StartsWith(context.Word, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (matchSpan.StartsWith("{_") && (match.Index < blockStartOffset || match.Index >= context.StartOffset))
            {
                continue;
            }

            string fullVariable = matchSpan.ToString();

            if (seenVariables.Add(fullVariable))
            {
                completions.Add(new CompletionData(
                    fullVariable,
                    fullVariable,
                    CompletionType.Variable,
                    null,
                    context.Word));
            }
        }
    }

    [GeneratedRegex(@"\{_?[a-zA-Z0-9_\.:]+\}")]
    private static partial Regex VariableRegex();
}