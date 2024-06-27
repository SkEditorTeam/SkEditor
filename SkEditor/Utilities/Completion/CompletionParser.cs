using System.Collections.Generic;
using System.Text.RegularExpressions;
using AvaloniaEdit.Snippets;

namespace SkEditor.Utilities.Completion;

public class CompletionParser
{
    private static readonly Regex ReplaceRegex = new(@"\[replace:(\w+)\]");
    private static readonly Regex BoundRegex = new(@"\[bound:(\w+)\]");
    private static readonly Regex CursorRegex = new(@"\[cursor\]");

    public static Snippet Parse(string template)
    {
        var snippet = new Snippet();
        var replaceableElements = new Dictionary<string, SnippetReplaceableTextElement>();

        int currentIndex = 0;
        while (currentIndex < template.Length)
        {
            var replaceMatch = ReplaceRegex.Match(template, currentIndex);
            var boundMatch = BoundRegex.Match(template, currentIndex);
            var cursorMatch = CursorRegex.Match(template, currentIndex);

            if (replaceMatch.Success && replaceMatch.Index == currentIndex)
            {
                var elementName = replaceMatch.Groups[1].Value;
                var replaceableElement = new SnippetReplaceableTextElement { Text = elementName.Replace("_", " ") };
                snippet.Elements.Add(replaceableElement);
                replaceableElements[elementName] = replaceableElement;
                currentIndex = replaceMatch.Index + replaceMatch.Length;
            }
            else if (boundMatch.Success && boundMatch.Index == currentIndex)
            {
                var elementName = boundMatch.Groups[1].Value;
                if (replaceableElements.TryGetValue(elementName, out var targetElement))
                {
                    snippet.Elements.Add(new SnippetBoundElement { TargetElement = targetElement });
                }
                currentIndex = boundMatch.Index + boundMatch.Length;
            }
            else if (cursorMatch.Success && cursorMatch.Index == currentIndex)
            {
                snippet.Elements.Add(new SnippetCaretElement());
                currentIndex = cursorMatch.Index + cursorMatch.Length;
            }
            else
            {
                int nextSpecialChar = template.IndexOfAny(new[] { '[' }, currentIndex);
                if (nextSpecialChar == -1) nextSpecialChar = template.Length;
                
                string text = template.Substring(currentIndex, nextSpecialChar - currentIndex);
                if (!string.IsNullOrEmpty(text))
                {
                    snippet.Elements.Add(new SnippetTextElement { Text = text });
                }
                currentIndex = nextSpecialChar;
            }
        }

        return snippet;
    }
}