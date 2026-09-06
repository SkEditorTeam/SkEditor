using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens;

public abstract class Token
{
    public abstract override string ToString();
    public abstract string ToCanonicalString(TypeRegistry typeRegistry);
    public abstract void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry);
}