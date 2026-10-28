using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Dialogs.Models.Elements;

[ObservableObject]
public partial class TextBoxDialogElement : DialogElement
{
    [ObservableProperty]
    public partial TranslatableString? ErrorMessage { get; set; }

    public TranslatableString? PlaceholderText { get; init; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    public Func<string, TranslatableString?>? ValidationFunc { get; init; }

    public bool Validate()
    {
        ErrorMessage = ValidationFunc?.Invoke(Text);
        return ErrorMessage is null;
    }
}