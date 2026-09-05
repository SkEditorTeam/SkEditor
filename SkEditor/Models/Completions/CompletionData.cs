using System;

namespace SkEditor.Models.Completions;

public class CompletionData
{
    public string Title { get; }
    public string Text { get; }
    public string? Description { get; }
    public string MatchedPrefix { get; }
    public string Remainder { get; }
    public CompletionType Type { get; }

    public CompletionData(string title, string text, CompletionType type, string? description = null,
        ReadOnlySpan<char> prefix = default)
    {
        Title = title;
        Text = text;
        Description = description;
        Type = type;

        if (!prefix.IsEmpty && title.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            MatchedPrefix = title[..prefix.Length];
            Remainder = title[prefix.Length..];
        }
        else
        {
            MatchedPrefix = string.Empty;
            Remainder = title;
        }
    }
}