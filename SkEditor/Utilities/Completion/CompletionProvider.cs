using AvaloniaEdit;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Completion;
public partial class CompletionProvider
{
    public static HashSet<CompletionItem> CompletionItems { get; set; } =
    [
        new CompletionItem("send", "send \"[cursor]\""),
        new CompletionItem("if", "if [cursor]:\n\t"),
        new CompletionItem("else", "else:\n\t"),
        new CompletionItem("ifelse", "if [cursor]:\n\t\nelse:\n\t"),
        
        new CompletionItem("command", "command /[replace:Name]:\n\ttrigger:\n\t\t"),
        new CompletionItem("function", "function [replace:Name]([replace:Arguments]):\n\t[cursor]"),
        new CompletionItem("function (with return)", "function [replace:Name]([replace:Arguments]) :: [replace:Return_Type]:\n\t[cursor]\n\treturn"),
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