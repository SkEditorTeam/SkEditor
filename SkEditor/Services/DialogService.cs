using System;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia.Fluent;
using SkEditor.Contracts.Services;
using SkEditor.Models.Dialogs;
using SkEditor.Services.Windowing;
using SkEditor.Views.Windows;

namespace SkEditor.Services;

public class DialogService(WindowRegistry windowRegistry) : IDialogService
{
    public async Task<DialogResult> ShowDialog(Dialog dialog)
    {
        MainWindow? mainWindow = windowRegistry.Active;

        if (mainWindow is null)
        {
            throw new InvalidOperationException("No active window found.");
        }

        var taskDialog = new FATaskDialog
        {
            Header = dialog.Title.Value,
            XamlRoot = mainWindow
        };

        if (dialog.Message is not null)
        {
            taskDialog.SubHeader = dialog.Message.Value;
        }

        if (dialog.Icon is not null)
        {
            taskDialog.IconSource = new SymbolIconSource { Symbol = dialog.Icon.Value };
        }

        foreach (DialogButton button in dialog.Buttons)
        {
            taskDialog.Buttons.Add(new FATaskDialogButton(button.Text.Value, button.Result));
        }

        object result = await taskDialog.ShowAsync(true);

        return result as DialogResult ?? throw new InvalidOperationException("Unexpected dialog result type.");
    }
}