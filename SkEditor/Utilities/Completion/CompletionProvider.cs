using AvaloniaEdit;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Completion;
public partial class CompletionProvider
{
    public static HashSet<CompletionItem> CompletionItems { get; set; } =
    [
        new("command", "command /{c}:\n\ttrigger:\n\t\t"),
        new("send", "send \"{c}\""),
        new("if", "if {c}:\n\t"),
        new("else", "else:\n\t"),
        new("ifelse", "if {c}:\n\t\nelse:\n\t"),
    ];


    public static IEnumerable<CompletionItem> GetCompletions(string word, TextEditor textEditor)
    {
        if (string.IsNullOrWhiteSpace(word)) yield break;
        foreach (CompletionItem completionItem in CompletionItems.Where(completionItem => completionItem.Name.StartsWith(word)))
        {
            yield return completionItem;
        }

        string text = textEditor.Text;
        foreach (Match match in VariableRegex().Matches(text).DistinctBy(x => x.Value))
        {
            string matchValue = match.Value;
            if (matchValue == word) continue;
            if (!matchValue.StartsWith(word)) continue;
            if (CompletionItems.Any(x => x.Name == matchValue)) continue;
            yield return new CompletionItem(matchValue, matchValue);
        }
    }

    [GeneratedRegex(@"{\w+}")]
    private static partial Regex VariableRegex();
}

public record CompletionItem(string Name, string Content);