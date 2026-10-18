using System;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Snippets;

namespace SkEditor.Models.Completions;

public class CompletionData
{
    public string Title { get; set; }
    public string? Text { get; }
    public string? Description { get; set; }
    public string MatchedPrefix { get; }
    public string Remainder { get; }
    public CompletionType Type { get; }
    public Snippet? Snippet { get; init; }
    public Action<TextArea, int, int>? CustomComplete { get; init; }

    public CompletionData(string title, string? text, CompletionType type, string? description = null,
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

    public void Complete(TextArea textArea, int startOffset, int lengthToReplace)
    {
        if (CustomComplete is not null)
        {
            CustomComplete(textArea, startOffset, lengthToReplace);
            return;
        }

        if (Snippet is not null)
        {
            textArea.Document.UndoStack.StartUndoGroup();
            try
            {
                textArea.Document.Remove(startOffset, lengthToReplace);
                Snippet.Insert(textArea);
            }
            finally
            {
                textArea.Document.UndoStack.EndUndoGroup();
            }

            return;
        }

        if (Text is not null)
        {
            textArea.Document.Replace(startOffset, lengthToReplace, Text);
        }
    }
}