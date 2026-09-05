using System;
using System.Collections.Generic;
using AvaloniaEdit.Document;
using SkEditor.Models.Completions;

namespace SkEditor.Utilities.Editor.Completions;

public class SectionCompletionProvider : ICompletionProvider
{
    private readonly CompletionData[] _sectionKeywords =
    [
        new("command", "command /"),
        new("function", "function ")
    ];

    public bool CheckCompletionsAvailable(TextDocument document, CompletionContext context)
    {
        return context.LineOffset == 0;
    }

    public void PopulateCompletions(TextDocument document, CompletionContext context, List<CompletionData> completions)
    {
        foreach (CompletionData data in _sectionKeywords)
        {
            if (data.Title.StartsWith(context.Word))
            {
                completions.Add(data);
            }
        }
    }
}