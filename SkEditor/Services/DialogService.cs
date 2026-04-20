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
            Header = "Niezapisane zmiany",
            SubHeader = "Plik 'adminchat.sk' ma niezapisane zmiany. Chcesz je zapisać przed zamknięciem?",
            IconSource = new SymbolIconSource { Symbol = Symbol.Save },
            Buttons =
            {
                new FATaskDialogButton("Zapisz", FATaskDialogStandardResult.Yes) { IsDefault = true },
                new FATaskDialogButton("Nie zapisuj", FATaskDialogStandardResult.No),
                FATaskDialogButton.CancelButton
            },
            XamlRoot = mainWindow
        };

        await dialog.ShowAsync(true);
    }
}