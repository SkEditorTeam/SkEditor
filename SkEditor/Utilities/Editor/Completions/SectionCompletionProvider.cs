using System;
using System.Collections.Generic;
using AvaloniaEdit.Document;
using SkEditor.Models.Completions;

namespace SkEditor.Utilities.Editor.Completions;

public class SectionCompletionProvider : ICompletionProvider
{
    private record CompletionItem(string Title, string Text);

    private readonly CompletionItem[] _sectionKeywords =
    [
        new("command", "command /"),
        new("function", "function "),
        new("options", "options:")
    ];

    public bool CheckCompletionsAvailable(TextDocument document, CompletionContext context)
    {
        return context.LineOffset == 0;
    }

    public void PopulateCompletions(TextDocument document, CompletionContext context, List<CompletionData> completions)
    {
        foreach (CompletionItem data in _sectionKeywords)
        {
            if (data.Title.StartsWith(context.Word, StringComparison.InvariantCultureIgnoreCase))
            {
                completions.Add(new CompletionData(data.Title, data.Text, CompletionType.Section, null, context.Word));
            }
        }
    }
}