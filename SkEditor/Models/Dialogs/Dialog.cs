using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Dialogs;

/// <summary>
///     Represents a dialog that can be displayed to the user.
/// </summary>
public class Dialog
{
    /// <summary>
    ///     The title of the dialog. This is a required property and must be set when creating a dialog.
    /// </summary>
    public required TranslatableString Title { get; init; }

    /// <summary>
    ///     The message of the dialog. This is an optional property and can be set using the <see cref="WithMessage" /> method.
    /// </summary>
    public TranslatableString? Message { get; set; }

    /// <summary>
    ///     The Symbol that will be used as the icon for the dialog. This is an optional property and can be set using the
    ///     <see cref="WithIcon" /> method.
    /// </summary>
    public Symbol? Icon { get; set; }

    /// <summary>
    ///     The buttons that will be displayed in the dialog. This is an optional property and can be set using the
    ///     <see cref="WithButtons" /> method.
    /// </summary>
    public List<DialogButton> Buttons { get; init; } = [];

    /// <summary>
    ///     Creates a new dialog with the specified title, message, and buttons.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message of the dialog.</param>
    /// <param name="buttons">The buttons that will be displayed in the dialog.</param
    /// <returns>>A new instance of the <see cref="Dialog" /> class with the specified title, message, and buttons.</returns>
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
    ///     Creates a new dialog with the specified title. The message and buttons can be set later using the
    ///     <see cref="WithMessage" /> and <see cref="WithButtons" /> methods.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <returns>A new instance of the <see cref="Dialog" /> class with the specified title.</returns>
    public static Dialog Create(TranslatableString title)
    {
        return new Dialog
        {
            Title = title
        };
    }

    /// <summary>
    ///     Sets the message of the dialog and returns the dialog instance for chaining. This method can be used to set the
    ///     message of the dialog after it has been created using the <see cref="Create(TranslatableString)" /> method.
    /// </summary>
    /// <param name="message"></param>
    /// <returns>>The dialog instance with the updated message.</returns>
    public Dialog WithMessage(TranslatableString message)
    {
        Message = message;
        return this;
    }

    /// <summary>
    ///     Sets the icon of the dialog and returns the dialog instance for chaining. This method can be used to set the icon
    ///     of the dialog after it has been created using the <see cref="Create(TranslatableString)" /> method.
    /// </summary>
    /// <param name="icon">The Symbol that will be used as the icon for the dialog.</param>
    /// <returns>The dialog instance with the updated icon.</returns>
    public Dialog WithIcon(Symbol icon)
    {
        Icon = icon;
        return this;
    }

    /// <summary>
    ///     Adds the specified buttons to the dialog and returns the dialog instance for chaining. This method can be used to
    ///     add buttons to the dialog after it has been created using the <see cref="Create(TranslatableString)" /> method.
    /// </summary>
    /// <param name="buttons"></param>
    /// <returns></returns>
    public Dialog WithButtons(params DialogButton[] buttons)
    {
        Buttons.AddRange(buttons);
        return this;
    }
}

/// <summary>
///     Represents a button that can be displayed in a dialog.
/// </summary>
/// <param name="text">The text that will be displayed on the button.</param>
/// <param name="result">The result that will be returned when the button is clicked.</param>
/// <param name="isDefault">Whether the button is the default button.</param>
public class DialogButton(TranslatableString text, DialogResult result, bool isDefault = false)
{
    /// <summary>
    ///     The text that will be displayed on the button.
    /// </summary>
    public TranslatableString Text { get; init; } = text;

    /// <summary>
    ///     Whether the button is the default button. If the user presses the Enter key while the dialog is open, the default
    ///     button will be clicked. Only one button can be the default button.
    /// </summary>
    public bool IsDefault { get; init; } = isDefault;

    /// <summary>
    ///     The result that will be returned when the button is clicked. This will be used to determine which button was
    ///     clicked when the dialog is closed.
    /// </summary>
    public DialogResult Result { get; init; } = result;

    /// <summary>
    ///     Predefined button for "OK". This button will return a <see cref="DialogResult" /> with a result of
    ///     <see cref="StandardDialogResult.Ok" /> when clicked.
    /// </summary>
    public static DialogButton Ok => new(TranslationKey.ButtonOkay, DialogResult.Ok);

    /// <summary>
    ///     Predefined button for "Cancel". This button will return a <see cref="DialogResult" /> with a result of
    ///     <see cref="StandardDialogResult.Cancel" /> when clicked.
    /// </summary>
    public static DialogButton Cancel => new(TranslationKey.ButtonCancel, DialogResult.Cancel);

    /// <summary>
    ///     Predefined button for "Yes". This button will return a <see cref="DialogResult" /> with a result of
    ///     <see cref="StandardDialogResult.Yes" /> when clicked.
    /// </summary>
    public static DialogButton Yes => new(TranslationKey.ButtonYes, DialogResult.Yes);

    /// <summary>
    ///     Predefined button for "No". This button will return a <see cref="DialogResult" /> with a result of
    ///     <see cref="StandardDialogResult.No" /> when clicked.
    /// </summary>
    public static DialogButton No => new(TranslationKey.ButtonNo, DialogResult.No);
}