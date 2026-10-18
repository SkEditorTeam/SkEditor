using AvaloniaEdit.Snippets;

namespace SkEditor.Editor.Completions.Models;

public class CompletionDataInfo(string title, string? text, CompletionType type, string? description = null)
{
    public string Title { get; } = title;
    public string? Text { get; } = text;
    public string? Description { get; } = description;
    public CompletionType Type { get; } = type;
    public Snippet? Snippet { get; init; }
}