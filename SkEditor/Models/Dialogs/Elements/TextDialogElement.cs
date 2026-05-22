using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Models.Dialogs.Elements;

[ObservableObject]
public partial class TextDialogElement : DialogElement
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public string? PlaceholderText { get; init; }

    public Func<string, string?>? ValidationFunc { get; init; }

    public bool Validate()
    {
        ErrorMessage = ValidationFunc?.Invoke(Text);
        return ErrorMessage is null;
    }
}