using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens.Base;

public class TokenGroup(params List<Token> tokens) : Token
{
    public List<Token> Tokens { get; } = tokens;

    public override string ToString()
    {
        return $"[GROUP]: \"{string.Join("", Tokens.Select(t => t.ToString()))}\"";
    }

    public override string ToCanonicalString(TypeRegistry typeRegistry)
    {
        return string.Join("", Tokens.Select(t => t.ToCanonicalString(typeRegistry)));
    }

    public override void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry)
    {
        foreach (Token token in Tokens)
        {
            token.BuildSnippet(snippet, typeRegistry);
        }
    }
}