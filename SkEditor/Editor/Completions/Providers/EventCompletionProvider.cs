using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using SkEditor.Core.Logging;
using SkEditor.Editor.Completions.Models;
using SkEditor.Matcher;
using SkEditor.Matcher.Models;

namespace SkEditor.Editor.Completions.Providers;

public class EventCompletionProvider : ICompletionProvider
{
    private volatile List<CompletionDataInfo> _cachedEvents = [];

    public EventCompletionProvider(ISkriptSyntaxService syntaxService, ILogService logs) =>
        _ = InitializeAsync(syntaxService, logs);

    private async Task InitializeAsync(ISkriptSyntaxService syntaxService, ILogService logs)
    {
        try
        {
            await syntaxService.InitializationTask.ConfigureAwait(false);

            List<CompletionDataInfo> events = PrecomputeEvents(syntaxService);
            _cachedEvents = events;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Error initializing event completion provider");
        }
    }

    private static List<CompletionDataInfo> PrecomputeEvents(ISkriptSyntaxService syntaxService)
    {
        List<CompletionDataInfo> infos = [];
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
                infos.Add(new CompletionDataInfo(canonical, canonical, CompletionType.Keyword, "snippet")
                {
                    Snippet = pattern.ToSnippet(syntaxService.TypeRegistry)
                });
            }
            else
            {
                infos.Add(new CompletionDataInfo(canonical, canonical, CompletionType.Keyword));
            }
        }

        infos.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
        return infos;
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