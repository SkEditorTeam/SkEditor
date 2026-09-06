using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens.Base;

public class RegexToken(string regexPattern) : Token
{
    public string RegexPattern { get; } = regexPattern;

    public override string ToString()
    {
        return $"[REGEX]: \"{RegexPattern}\"";
    }

    public override string ToCanonicalString(TypeRegistry typeRegistry)
    {
        return $"<{RegexPattern}>";
    }

    public override void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry)
    {
        snippet.Elements.Add(new SnippetTextElement { Text = RegexPattern });
    }
}