using System;
using System.Collections.Generic;
using AvaloniaEdit.Document;
using AvaloniaEdit.Snippets;
using SkEditor.Models.Completions;
using SkEditor.Utilities.Extensions;

namespace SkEditor.Utilities.Editor.Completions;

public class SectionCompletionProvider : ICompletionProvider
{
    private readonly List<CompletionDataInfo> _completions = [];

    public SectionCompletionProvider()
    {
        _completions.Add(new CompletionDataInfo("options", "options", CompletionType.Section));

        _completions.Add(new CompletionDataInfo("function", "function", CompletionType.Section));
        _completions.Add(new CompletionDataInfo("function", null, CompletionType.Section)
        {
            Snippet = new Snippet()
                .Text("function ")
                .Placeholder("myFunction")
                .Text("(")
                .Placeholder("p: player")
                .Text("):\n\t")
                .Caret()
        });

        _completions.Add(new CompletionDataInfo("command", "command", CompletionType.Section));
        _completions.Add(new CompletionDataInfo("command", null, CompletionType.Section)
        {
            Snippet = new Snippet()
                .Text("command /")
                .Placeholder("name")
                .Text(":\n\ttrigger:\n\t\t")
                .Caret()
        });
    }

    public bool CheckCompletionsAvailable(TextDocument document, CompletionContext context)
    {
        return context.LineOffset == 0;
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