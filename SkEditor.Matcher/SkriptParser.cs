using SkEditor.Matcher.Models;
using SkEditor.Matcher.Registries;
using SkEditor.Matcher.Tokens;
using SkEditor.Matcher.Tokens.Base;

namespace SkEditor.Matcher;

public class SkriptParser
{
    public List<Token> Parse(ReadOnlySpan<char> input, SyntaxElement context, TypeRegistry typeRegistry)
    {
        List<int> choiceSplits = [];
        int bracketDepth = 0;
        bool inRegex = false;
        bool inExpr = false;
        bool escaped = false;

        for (int i = 0; i < input.Length; i++)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (input[i] == '\\')
            {
                escaped = true;
                continue;
            }

            switch (input[i])
            {
                case '[' or '(': bracketDepth++; break;
                case ']' or ')': bracketDepth--; break;
                case '<': inRegex = true; break;
                case '>': inRegex = false; break;
                case '%': inExpr = !inExpr; break;
                case '|' when bracketDepth == 0 && !inRegex && !inExpr:
                    choiceSplits.Add(i);
                    break;
            }
        }

        if (choiceSplits.Count > 0)
        {
            List<Token> choices = [];
            int lastSplit = 0;
            foreach (int split in choiceSplits)
            {
                List<Token> subTokens = Parse(input.Slice(lastSplit, split - lastSplit), context, typeRegistry);
                choices.Add(subTokens.Count == 1 ? subTokens[0] : new TokenGroup(subTokens));
                lastSplit = split + 1;
            }

            List<Token> finalTokens = Parse(input[lastSplit..], context, typeRegistry);
            choices.Add(finalTokens.Count == 1 ? finalTokens[0] : new TokenGroup(finalTokens));
            return [new ChoiceToken(choices)];
        }

        List<Token> tokens = [];
        int literalStart = 0;

        void AddLiteral(int endIndex, ReadOnlySpan<char> literalInput)
        {
            if (endIndex <= literalStart)
            {
                return;
            }

            string val = literalInput.Slice(literalStart, endIndex - literalStart).ToString();
            val = val.Replace("\\", "");
            int tagIndex = val.LastIndexOf(':');
            if (tagIndex == -1)
            {
                tagIndex = val.LastIndexOf('¦');
            }

            if (tagIndex != -1)
            {
                val = val[(tagIndex + 1)..];
            }

            if (!string.IsNullOrEmpty(val))
            {
                tokens.Add(new TagToken(val));
            }
        }

        escaped = false;
        for (int i = 0; i < input.Length; i++)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (input[i] == '\\')
            {
                escaped = true;
                continue;
            }

            switch (input[i])
            {
                case '%':
                {
                    AddLiteral(i, input);
                    for (int j = i + 1; j < input.Length; j++)
                    {
                        if (input[j] == '\\')
                        {
                            j++;
                            continue;
                        }

                        if (input[j] != '%')
                        {
                            continue;
                        }

                        if (j == i + 1)
                        {
                            tokens.Add(new LiteralToken("%"));
                        }
                        else
                        {
                            tokens.Add(BuildExpressionToken(input.Slice(i + 1, j - i - 1), typeRegistry));
                        }

                        i = j;
                        literalStart = i + 1;
                        break;
                    }

                    break;
                }
                case '[' or '(':
                {
                    char startChar = input[i];
                    char endChar = startChar == '[' ? ']' : ')';

                    AddLiteral(i, input);
                    int depth = 0;
                    for (int j = i + 1; j < input.Length; j++)
                    {
                        if (input[j] == '\\')
                        {
                            j++;
                            continue;
                        }

                        if (input[j] == startChar)
                        {
                            depth++;
                        }
                        else if (input[j] == endChar)
                        {
                            if (depth == 0)
                            {
                                ReadOnlySpan<char> innerContent = input.Slice(i + 1, j - i - 1);
                                List<Token> innerTokens = Parse(innerContent, context, typeRegistry);

                                if (startChar == '[')
                                {
                                    tokens.Add(new OptionalToken(innerTokens.Count == 1
                                        ? innerTokens[0]
                                        : new TokenGroup(innerTokens)));
                                }
                                else
                                {
                                    tokens.AddRange(innerTokens);
                                }

                                i = j;
                                literalStart = i + 1;
                                break;
                            }

                            depth--;
                        }
                    }

                    break;
                }
                case '<':
                {
                    AddLiteral(i, input);
                    for (int j = i + 1; j < input.Length; j++)
                    {
                        if (input[j] == '\\')
                        {
                            j++;
                            continue;
                        }

                        if (input[j] != '>')
                        {
                            continue;
                        }

                        string regexContent = input.Slice(i + 1, j - i - 1).ToString();
                        regexContent = regexContent.Replace("\\>", ">");

                        tokens.Add(new RegexToken(regexContent));

                        i = j;
                        literalStart = i + 1;
                        break;
                    }

                    break;
                }
            }
        }

        AddLiteral(input.Length, input);
        return tokens;
    }

    private static ExpressionToken BuildExpressionToken(ReadOnlySpan<char> placeholderContent,
        TypeRegistry typeRegistry)
    {
        bool nullable = false;
        bool forbidLiterals = false;
        bool mustBeLiteral = false;

        int contentStart = 0;
        while (contentStart < placeholderContent.Length)
        {
            switch (placeholderContent[contentStart])
            {
                case '-':
                    nullable = true;
                    contentStart++;
                    continue;
                case '~':
                    forbidLiterals = true;
                    contentStart++;
                    continue;
                case '*':
                    mustBeLiteral = true;
                    contentStart++;
                    continue;
            }

            break;
        }

        ReadOnlySpan<char> expressionType = placeholderContent[contentStart..];
        int atIndex = expressionType.IndexOf('@');
        if (atIndex != -1)
        {
            expressionType = expressionType[..atIndex];
        }

        List<string> expressions = [];
        if (!expressionType.Contains('/'))
        {
            expressions.Add(expressionType.ToString());
        }
        else
        {
            int lastSlashIndex = 0;
            for (int i = 0; i < expressionType.Length; i++)
            {
                if (expressionType[i] != '/')
                {
                    continue;
                }

                expressions.Add(expressionType.Slice(lastSlashIndex, i - lastSlashIndex).ToString());
                lastSlashIndex = i + 1;
            }

            if (lastSlashIndex < expressionType.Length)
            {
                expressions.Add(expressionType[lastSlashIndex..].ToString());
            }
        }

        List<TypeToken> typeTokens = [];
        foreach (string str in expressions)
        {
            typeTokens.Add(typeRegistry.TryParseToken(str, out TypeToken t) ? t : TypeToken.Object);
        }

        return new ExpressionToken(typeTokens)
        {
            Nullable = nullable,
            ForbidLiterals = forbidLiterals,
            MustBeLiteral = mustBeLiteral
        };
    }
}