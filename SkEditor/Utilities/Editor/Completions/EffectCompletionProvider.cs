using System;
using System.Collections.Generic;
using AvaloniaEdit.Document;
using SkEditor.Models.Completions;

namespace SkEditor.Utilities.Editor.Completions;

public class EffectCompletionProvider : ICompletionProvider
{
    private readonly List<CompletionDataInfo> _completions = [];

    public EffectCompletionProvider()
    {
        _completions.Add(new CompletionDataInfo("set", "set", CompletionType.Keyword));
        ;
        _completions.Add(new CompletionDataInfo("clear", "clear", CompletionType.Keyword));
        ;
    }

    public bool CheckCompletionsAvailable(TextDocument document, CompletionContext context)
    {
        int offset = context.StartOffset;
        DocumentLine line = document.GetLineByOffset(offset);

        int lineStartOffset = line.Offset;

        ReadOnlySpan<char> beforeWord = document.GetTextAsMemory(lineStartOffset, offset - lineStartOffset).Span;

        return beforeWord.Length > 0 && beforeWord.IsWhiteSpace();
    }

    public void PopulateCompletions(TextDocument document, CompletionContext context, List<CompletionData> completions)
    {
        foreach (CompletionDataInfo info in _completions)
        {
            if (!info.Title.StartsWith(context.Word, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            completions.Add(new CompletionData(info.Title, info.Text, info.Type, info.Description, context.Word)
            {
                Snippet = info.Snippet,
                Description = info.Snippet is not null ? "snippet" : info.Description
            });
        }
    }
}