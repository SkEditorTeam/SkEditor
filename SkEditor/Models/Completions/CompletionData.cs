namespace SkEditor.Models.Completions;

public class CompletionData(string title, string text, CompletionType type = CompletionType.Text)
{
    public string Title { get; set; } = title;
    public string Text { get; set; } = text;
    public CompletionType Type { get; set; } = type;
}