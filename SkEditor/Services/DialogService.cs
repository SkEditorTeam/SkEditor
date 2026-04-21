using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Services.Windowing;
using SkEditor.Views.Windows;

namespace SkEditor.Services;

public class DialogService(WindowRegistry windowRegistry, ILogService logs) : IDialogService
{
    public async Task Debug(string message)
    {
        MainWindow? mainWindow = windowRegistry.Active;

        if (mainWindow is null)
        {
            throw new InvalidOperationException("No active window found.");
        }

        var dialog = new FATaskDialog
        {
            Header = "Unsaved changes",
            SubHeader = "This file has unsaved changes. Do you want to save them before closing?",
            IconSource = new SymbolIconSource { Symbol = Symbol.Save },
            Buttons =
            {
                new FATaskDialogButton("Save", FATaskDialogStandardResult.Yes) { IsDefault = true },
                new FATaskDialogButton("Don't Save", FATaskDialogStandardResult.No),
                new FATaskDialogButton("Cancel", FATaskDialogStandardResult.Cancel),
            },
            XamlRoot = mainWindow
        };

        await dialog.ShowAsync(true);
    }
}