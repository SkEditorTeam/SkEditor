using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens.Base;

public class OptionalToken(params List<Token> tokens) : Token
{
    public List<Token> Tokens { get; } = tokens;

    public override string ToString()
    {
        return $"[OPT]: {string.Join(" ", Tokens)}";
    }

    public override string ToCanonicalString(TypeRegistry typeRegistry)
    {
        return string.Empty;
    }

    public override void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry)
    {
    }
}