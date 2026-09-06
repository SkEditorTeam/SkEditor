using System.Text;
using System.Text.RegularExpressions;
using AvaloniaEdit.Snippets;
using SkEditor.Matcher.Registries;
using SkEditor.Matcher.Tokens;
using SkEditor.Matcher.Tokens.Base;

namespace SkEditor.Matcher;

public partial class SyntaxPattern(List<Token> tokens)
{
    public List<Token> Tokens { get; } = tokens;

    public bool HasExpressions => Tokens.Any(t =>
        t is ExpressionToken || (t is TokenGroup g && g.Tokens.Any(gt => gt is ExpressionToken)));

    public string ToCanonicalString(TypeRegistry typeRegistry)
    {
        StringBuilder sb = new();
        foreach (Token token in Tokens)
        {
            sb.Append(token.ToCanonicalString(typeRegistry));
        }

        string raw = sb.ToString();
        return WhitespaceRegex().Replace(raw, " ").Trim();
    }

    public Snippet ToSnippet(TypeRegistry typeRegistry)
    {
        Snippet snippet = new();
        foreach (Token token in Tokens)
        {
            token.BuildSnippet(snippet, typeRegistry);
        }

        return snippet;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}