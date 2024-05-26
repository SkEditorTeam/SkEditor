﻿using SkEditor.Views;

namespace SkEditor.API;

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

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
    /// <returns>The result of the dialog.</returns>
    Task<ContentDialogResult> ShowDialog(string title,
        string message,
        object? icon = null,
        string? cancelButtonText = null,
        string primaryButtonText = "Okay");

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
}