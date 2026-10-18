using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Core.Dialogs.Models.Elements;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Dialogs.Models;

/// <summary>
///     Represents a customizable dialog window to be displayed in the SkEditor UI.
/// </summary>
public class Dialog
{
    /// <summary> Gets or sets whether the user can close the dialog by pressing the Esc key. </summary>
    public bool AllowIgnore { get; set; } = true;

    /// <summary> The interactive buttons displayed at the bottom of the dialog. </summary>
    public List<DialogButton> Buttons { get; init; } = [];

    /// <summary> Additional UI elements or custom controls embedded inside the dialog body. </summary>
    public List<DialogElement> Elements { get; init; } = [];

    /// <summary> The optional icon displayed alongside the dialog content. </summary>
    public Symbol? Icon { get; set; }

    /// <summary> The optional localized description body of the dialog. </summary>
    public TranslatableString? Message { get; set; }

    /// <summary> The localized title displayed in the header of the dialog. </summary>
    public required TranslatableString Title { get; init; }

    /// <summary>
    ///     Creates a new dialog with a title, message, and predefined buttons.
    /// </summary>
    /// <param name="title">The localized title of the dialog.</param>
    /// <param name="message">The localized content of the dialog body.</param>
    /// <param name="buttons">The interactive buttons to be appended to the dialog bottom.</param>
    /// <returns>A configured <see cref="Dialog" /> instance ready to be shown.</returns>
    public static Dialog Create(TranslatableString title, TranslatableString message, params DialogButton[] buttons)
    {
        return new Dialog
        {
            Title = title,
            Message = message,
            Buttons = [..buttons]
        };
    }

    /// <summary>
    ///     Creates a basic dialog with only a title. Other properties can be configured via fluent methods.
    /// </summary>
    /// <param name="title">The localized title of the dialog.</param>
    /// <returns>A minimal <see cref="Dialog" /> instance.</returns>
    public static Dialog Create(TranslatableString title)
    {
        return new Dialog
        {
            Title = title
        };
    }

    /// <summary> Sets the message body of the dialog and returns this instance for chaining. </summary>
    public Dialog WithMessage(TranslatableString message)
    {
        Message = message;
        return this;
    }

    /// <summary> Sets the icon of the dialog and returns this instance for chaining. </summary>
    public Dialog WithIcon(Symbol icon)
    {
        Icon = icon;
        return this;
    }

    /// <summary> Appends buttons to the dialog and returns this instance for chaining. </summary>
    public Dialog WithButtons(params DialogButton[] buttons)
    {
        Buttons.AddRange(buttons);
        return this;
    }

    /// <summary> Appends custom UI elements to the dialog body and returns this instance for chaining. </summary>
    public Dialog WithElements(params DialogElement[] elements)
    {
        Elements.AddRange(elements);
        return this;
    }

    /// <summary> Prevents the dialog from being closed by pressing the Esc key and returns this instance for chaining. </summary>
    public Dialog DisallowIgnore()
    {
        AllowIgnore = false;
        return this;
    }
}

/// <summary>
///     Represents an interactive button within a dialog.
/// </summary>
public class DialogButton(TranslatableString text, DialogResult result, bool isDefault = false)
{
    /// <summary> Predefined button for 'Cancel'. </summary>
    public static DialogButton Cancel => new(TranslationKey.ButtonCancel, DialogResult.Cancel);

    /// <summary> Gets whether pressing Enter triggers this button. Only one button in a dialog should be default. </summary>
    public bool IsDefault { get; init; } = isDefault;

    /// <summary> Predefined button for 'No'. </summary>
    public static DialogButton No => new(TranslationKey.ButtonNo, DialogResult.No);

    /// <summary> Predefined button for 'OK'. </summary>
    public static DialogButton Ok => new(TranslationKey.ButtonOkay, DialogResult.Ok);

    /// <summary> The result value returned to the caller when this button is clicked. </summary>
    public DialogResult Result { get; init; } = result;

    /// <summary> The localized label displayed on the button. </summary>
    public TranslatableString Text { get; init; } = text;

    /// <summary> Predefined button for 'Yes'. </summary>
    public static DialogButton Yes => new(TranslationKey.ButtonYes, DialogResult.Yes);
}