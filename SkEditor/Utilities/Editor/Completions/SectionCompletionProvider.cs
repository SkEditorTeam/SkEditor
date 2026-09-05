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

        CompletionDataInfo functionCompletion = new("function", null, CompletionType.Function)
        {
            Snippet = new Snippet()
                .Text("function ")
                .Placeholder("myFunction")
                .Text("(")
                .Placeholder("p: player")
                .Text("):\n\t")
                .Caret()
        };
        _completions.Add(functionCompletion);
    }

    public bool CheckCompletionsAvailable(TextDocument document, CompletionContext context)
    {
        return context.LineOffset == 0;
    }

    public void PopulateCompletions(TextDocument document, CompletionContext context, List<CompletionData> completions)
    {
        foreach (CompletionDataInfo info in _completions)
        {
            if (!info.Title.StartsWith(context.Word, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            CompletionData completionData = new(info.Title, info.Text, CompletionType.Section, null, context.Word);

            if (info.Snippet is not null)
            {
                completionData.CustomComplete = (textArea, startOffset, lengthToReplace) =>
                {
                    textArea.Document.Remove(startOffset, lengthToReplace);
                    info.Snippet?.Insert(textArea);
                };

                completionData.Description = "snippet";
            }

            completions.Add(completionData);
        }
    }
}