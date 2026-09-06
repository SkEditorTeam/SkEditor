using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens.Base;

public class LiteralToken(string value) : Token
{
    public string Value { get; } = value;

    public override string ToString()
    {
        return $"[LIT]: \"{Value}\"";
    }

    public override string ToCanonicalString(TypeRegistry typeRegistry)
    {
        return Value;
    }

    public override void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry)
    {
        snippet.Elements.Add(new SnippetTextElement { Text = Value });
    }
}