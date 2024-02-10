using AvaloniaEdit;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Completion;
public partial class CompletionProvider
{
    public static HashSet<CompletionItem> CompletionItems { get; set; } =
    [
        new CompletionItem("command", "command /{c}:\n\ttrigger:\n\t\t"),
        new CompletionItem("send", "send \"{c}\""),
        new CompletionItem("if", "if {c}:\n\t"),
        new CompletionItem("else", "else:\n\t"),
        new CompletionItem("ifelse", "if {c}:\n\t\nelse:\n\t"),
    ];


    public static IEnumerable<CompletionItem> GetCompletions(string word, TextEditor textEditor)
    {
        if (string.IsNullOrWhiteSpace(word)) yield break;
        foreach (var completionItem in CompletionItems)
        {
            if (!completionItem.Name.StartsWith(word)) continue;
            yield return completionItem;
        }

        //string text = textEditor.Text;
        //foreach (Match match in WordRegex().Matches(text).Cast<Match>())
        //{
        //    string matchValue = match.Value;
        //    if (matchValue == word) continue;
        //    if (!matchValue.StartsWith(word)) continue;
        //    if (CompletionItems.Any(x => x.Name == matchValue)) continue;
        //    yield return new CompletionItem(matchValue, matchValue);
        //}
    }

    [GeneratedRegex(@"\w+")]
    private static partial Regex WordRegex();
}

public record CompletionItem(string Name, string Content);