namespace SkEditor.Models.Dialogs.Elements;

public class TextDialogElement : DialogElement
{
    public string Text { get; init; } = string.Empty;
    public string? PlaceholderText { get; init; }
}