using System;
using System.Collections.Generic;
using AvaloniaEdit.Document;
using SkEditor.Matcher;
using SkEditor.Matcher.Models;
using SkEditor.Models.Completions;

namespace SkEditor.Utilities.Editor.Completions;

public class EventCompletionProvider : ICompletionProvider
{
    private readonly List<CompletionDataInfo> _cachedEvents = [];

    public EventCompletionProvider(ISkriptSyntaxService syntaxService) => PrecomputeEvents(syntaxService);

    private void PrecomputeEvents(ISkriptSyntaxService syntaxService)
    {
        HashSet<string> seenCanonicals = new(StringComparer.OrdinalIgnoreCase);

        foreach ((_, SyntaxPattern pattern) in syntaxService.GetSyntaxByType(SyntaxElementDto.SyntaxType.Event))
        {
            string canonical = pattern.ToCanonicalString(syntaxService.TypeRegistry);
            if (string.IsNullOrWhiteSpace(canonical))
            {
                continue;
            }

            if (canonical.StartsWith("on ", StringComparison.OrdinalIgnoreCase))
            {
                canonical = canonical[3..].TrimStart();
            }

            if (!seenCanonicals.Add(canonical))
            {
                continue;
            }

            if (pattern.HasExpressions)
            {
                _cachedEvents.Add(new CompletionDataInfo(canonical, canonical, CompletionType.Keyword, "snippet")
                {
                    Snippet = pattern.ToSnippet(syntaxService.TypeRegistry)
                });
            }
            else
            {
                _cachedEvents.Add(new CompletionDataInfo(canonical, canonical, CompletionType.Keyword));
            }
        }

        _cachedEvents.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
    }

    public bool CheckCompletionsAvailable(TextDocument document, CompletionContext context)
    {
        DocumentLine line = document.GetLineByOffset(context.StartOffset);

        string textBeforeWord = document.GetText(line.Offset, context.StartOffset - line.Offset);
        return textBeforeWord.Equals("on ", StringComparison.OrdinalIgnoreCase);
    }

    public void PopulateCompletions(TextDocument document, CompletionContext context, List<CompletionData> completions)
    {
        foreach (CompletionDataInfo info in _cachedEvents)
        {
            if (!info.Title.StartsWith(context.Word, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            completions.Add(new CompletionData(info.Title, info.Text, info.Type, info.Description, context.Word)
            {
                Snippet = info.Snippet
            });
        }
    }
}