using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Dialogs.Elements;

[ObservableObject]
public partial class TextDialogElement : DialogElement
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial TranslatableString? ErrorMessage { get; set; }

    public TranslatableString? PlaceholderText { get; init; }

    public Func<string, TranslatableString?>? ValidationFunc { get; init; }

    public bool Validate()
    {
        ErrorMessage = ValidationFunc?.Invoke(Text);
        return ErrorMessage is null;
    }
}