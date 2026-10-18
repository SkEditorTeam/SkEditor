using System.Collections.Generic;
using AvaloniaEdit.Document;
using SkEditor.Models.Completions;

namespace SkEditor.Utilities.Editor.Completions;

public interface ICompletionProvider
{
    bool CheckCompletionsAvailable(TextDocument document, CompletionContext context);

    void PopulateCompletions(TextDocument document, CompletionContext context,
        List<CompletionData> completions);
}