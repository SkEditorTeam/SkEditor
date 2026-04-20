using System;
using System.Threading.Tasks;
using Avalonia.Controls;
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

        FATaskDialog td = new()
        {
            Title = "FluentAvalonia",
            Header = "Header",
            SubHeader = "Subheader",
            Content = "This is some sample text, but you can put more advanced content here if you like",
            IconSource = new SymbolIconSource { Symbol = Symbol.Save },
            FooterVisibility = FATaskDialogFooterVisibility.Auto,
            Footer = new CheckBox { Content = "Never show me this again" },
            Commands =
            {
                new FATaskDialogCommand
                {
                    Text = "Command Text",
                    Description = "Description",
                    DialogResult = "CommandResult",
                    ClosesOnInvoked = true
                }
            },
            Buttons =
            {
                new FATaskDialogButton("OK", "myResult")
            },
            XamlRoot = mainWindow
        };

        await td.ShowAsync();
    }
}