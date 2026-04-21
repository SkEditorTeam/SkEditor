using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Dialogs;

public class Dialog
{
    public required TranslatableString Title { get; init; }
    public TranslatableString? Message { get; set; }
    public Symbol? Icon { get; set; }

    public List<DialogButton> Buttons { get; init; } = [];

    public static Dialog Create(TranslatableString title, TranslatableString message, params DialogButton[] buttons)
    {
        return new Dialog
        {
            Title = title,
            Message = message,
            Buttons = [..buttons]
        };
    }

    public static Dialog Create(TranslatableString title)
    {
        return new Dialog
        {
            Title = title
        };
    }

    public Dialog WithMessage(TranslatableString message)
    {
        Message = message;
        return this;
    }

    public Dialog WithIcon(Symbol icon)
    {
        Icon = icon;
        return this;
    }

    public Dialog WithButtons(params DialogButton[] buttons)
    {
        Buttons.AddRange(buttons);
        return this;
    }
}

public class DialogButton(TranslatableString text, DialogResult result, bool isDefault = false)
{
    public TranslatableString Text { get; init; } = text;
    public bool IsDefault { get; init; } = isDefault;
    public DialogResult Result { get; init; } = result;

    public static DialogButton Ok => new(TranslationKey.ButtonOkay, DialogResult.Ok);
    public static DialogButton Cancel => new(TranslationKey.ButtonCancel, DialogResult.Cancel);
    public static DialogButton Yes => new(TranslationKey.ButtonYes, DialogResult.Yes);
    public static DialogButton No => new(TranslationKey.ButtonNo, DialogResult.No);
}