using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia.Fluent;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Dialogs.Models.Elements;
using SkEditor.Windowing;
using SkEditor.Windowing.UI;

namespace SkEditor.Core.Dialogs;

public class DialogService(WindowRegistry windowRegistry) : IDialogService
{
    public async Task<DialogResult> ShowDialogAsync(Dialog dialog)
    {
        MainWindow? mainWindow = windowRegistry.Active;

        if (mainWindow is null)
        {
            throw new InvalidOperationException("No active window found.");
        }

        FATaskDialog taskDialog = BuildTaskDialog(dialog);
        taskDialog.XamlRoot = windowRegistry.FindTopmostWindow(mainWindow);

        taskDialog.Focus();

        object result = await taskDialog.ShowAsync(true);

        return result as DialogResult ?? DialogResult.Cancel;
    }

    public async Task<DialogResult> RunDialogAsync(Dialog dialog, Func<CancellationToken, Task> work)
    {
        MainWindow? mainWindow = windowRegistry.Active;

        if (mainWindow is null)
        {
            throw new InvalidOperationException("No active window found.");
        }

        FATaskDialog taskDialog = BuildTaskDialog(dialog);
        taskDialog.XamlRoot = windowRegistry.FindTopmostWindow(mainWindow);

        using CancellationTokenSource cts = new();
        
        Task<object> dialogTask = taskDialog.ShowAsync(true);
        Task workTask = work(cts.Token);

        Task completedTask = await Task.WhenAny(dialogTask, workTask);
        
        if (completedTask == workTask)
        {
            await workTask;
            taskDialog.Hide(DialogResult.Ok);
        
            await dialogTask;
            return DialogResult.Ok;
        }

        await cts.CancelAsync();

        try
        {
            await workTask;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }

        object rawResult = await dialogTask;
        return rawResult as DialogResult ?? DialogResult.Cancel;
    }

    private static FATaskDialog BuildTaskDialog(Dialog dialog)
    {
        FATaskDialog taskDialog = new()
        {
            Header = dialog.Title.Value,
            Focusable = true
        };

        if (dialog.Message is not null)
        {
            taskDialog.SubHeader = dialog.Message.Value;
        }

        if (dialog.Icon is not null)
        {
            taskDialog.IconSource = new SymbolIconSource { Symbol = dialog.Icon.Value, FontSize = 28};
        }

        if (dialog.Elements.Count > 0)
        {
            taskDialog.Content = new ItemsControl
            {
                ItemsPanel = new FuncTemplate<Panel?>(() =>
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 8
                    }),
                ItemsSource = dialog.Elements
            };

            TextBoxDialogElement? firstText = dialog.Elements.OfType<TextBoxDialogElement>().FirstOrDefault();
            firstText?.AutoFocus = true;
        }

        taskDialog.Closing += OnTaskDialogOnClosing;

        foreach (DialogButton button in dialog.Buttons)
        {
            taskDialog.Buttons.Add(new FATaskDialogButton(button.Text.Value, button.Result)
                { IsDefault = button.IsDefault });
        }

        return taskDialog;

        void OnTaskDialogOnClosing(FATaskDialog _, FATaskDialogClosingEventArgs args)
        {
            if (args.Result is FATaskDialogStandardResult.None)
            {
                if (!dialog.AllowIgnore)
                {
                    args.Cancel = true;
                }

                return;
            }

            if (DialogResult.Cancel.Equals(args.Result))
            {
                return;
            }

            if (dialog.Elements.Any(e => e is TextBoxDialogElement { ValidationFunc: not null }))
            {
                bool valid = dialog.Elements.OfType<TextBoxDialogElement>()
                    .Select(e => e.Validate())
                    .Aggregate(true, (acc, v) => acc && v);

                args.Cancel = !valid;
            }

            if (!args.Cancel)
            {
                taskDialog.Closing -= OnTaskDialogOnClosing;
            }
        }
    }
}