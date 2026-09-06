using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Models;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Tokens.Base;

public class ExpressionToken(params List<TypeToken> types) : Token
{
    public List<TypeToken> Types { get; } = types;
    public bool Nullable { get; init; }
    public bool ForbidLiterals { get; init; }
    public bool MustBeLiteral { get; init; }

    public override string ToString()
    {
        return $"[EXP]: \"{string.Join("/", Types)}\"";
    }

    public override string ToCanonicalString(TypeRegistry typeRegistry)
    {
        string typeName = Types.Count > 0 ? typeRegistry.GetName(Types[0]) : "object";
        return $"<{typeName}>";
    }

    public override void BuildSnippet(Snippet snippet, TypeRegistry typeRegistry)
    {
        string typeName = Types.Count > 0 ? typeRegistry.GetName(Types[0]) : "object";
        snippet.Elements.Add(new SnippetReplaceableTextElement { Text = $"<{typeName}>" });
    }
}