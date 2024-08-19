using Avalonia.Platform.Storage;
using SkEditor.Views;

namespace SkEditor.API;

using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

/// <summary>
/// Interface for Windows-related operations.
/// </summary>
public interface IWindows
{
    /// <summary>
    /// Get the main SkEditor window.
    /// </summary>
    /// <returns>The main SkEditor window.</returns>
    MainWindow GetMainWindow();

    /// <summary>
    /// Get the current top-level window. This may not be the main window.
    /// </summary>
    /// <returns>The current top-level window.</returns>
    Window GetCurrentWindow();

    /// <summary>
    /// Show a dialog to the user, and waits for its answer. In this method, we'll
    /// try to translate the given title & message. 
    /// </summary>
    /// <param name="title">The dialog's title.</param>
    /// <param name="message">The dialog's message.</param>
    /// <param name="icon">The dialog's icon. Can be null.</param>
    /// <param name="cancelButtonText">The text of the cancel button. If null, no cancel button will be shown.</param>
    /// <param name="primaryButtonText">The text of the primary button. Default is "Okay", <b>cannot be null</b></param>
    /// <param name="translate">Whether to try translate the title</param>
    /// <returns>The result of the dialog.</returns>
    Task<ContentDialogResult> ShowDialog(string title,
        string message,
        object? icon = null,
        string? cancelButtonText = null,
        string primaryButtonText = "Okay", bool translate = true);

    /// <summary>
    /// Show a message dialog to the user.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message of the dialog.</param>
    Task ShowMessage(string title, string message);

    /// <summary>
    /// Show an error dialog to the user.
    /// </summary>
    /// <param name="error">The error message.</param>
    Task ShowError(string error);

    /// <summary>
    /// Ask the user to select a file with the given options.
    /// </summary>
    /// <param name="options">The options for the file picker.</param>
    /// <returns>The path of the selected file, or null if the user cancelled the dialog.</returns>
    Task<string?> AskForFile(FilePickerOpenOptions options);

    /// <summary>
    /// Show the specific window at the most top level possible (so not always on top the main window!)
    /// </summary>
    /// <param name="window">The window to show</param>
    void ShowWindow(Window window);

    /// <summary>
    /// Show the specific window at the most top level possible (so not always on top the main window!)
    /// This will wait for the window to be closed before continuing.
    /// </summary>
    /// <param name="window">The window to show</param>
    /// <returns>The task that will be completed when the window is closed.</returns>
    Task ShowWindowAsDialog(Window window);



}