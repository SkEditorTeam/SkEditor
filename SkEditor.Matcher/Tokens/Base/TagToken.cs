using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens.Base;

public class TagToken(string literalValue) : Token
{
    public string LiteralValue { get; } = literalValue;

    public override string ToString()
    {
        return $"[TAG: {LiteralValue}]";
    }

    public override string ToCanonicalString(TypeRegistry typeRegistry)
    {
        return LiteralValue;
    }

    public override void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry)
    {
        snippet.Elements.Add(new SnippetTextElement { Text = LiteralValue });
    }
}