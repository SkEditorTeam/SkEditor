using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens.Base;

public class ChoiceToken(params List<Token> options) : Token
{
    public List<Token> Options { get; } = options;

    public override string ToString()
    {
        return $"[CHOICE]: {string.Join(" ", Options)}";
    }

    public override string ToCanonicalString(TypeRegistry typeRegistry)
    {
        return Options.Count > 0 ? Options[0].ToCanonicalString(typeRegistry) : string.Empty;
    }

    public override void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry)
    {
        if (Options.Count > 0)
        {
            Options[0].BuildSnippet(snippet, typeRegistry);
        }
    }
}