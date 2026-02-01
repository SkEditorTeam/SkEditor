using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Threading;
using SkEditor.Utilities.Completion;

namespace SkEditor.Views.Controls;

public partial class CompletionMenu : UserControl
{
    public CompletionMenu(IEnumerable<CompletionItem> completions)
    {
        InitializeComponent();

        SetItems(completions);
    }

    [Obsolete("This constructor is for designer use only.", true)]
    public CompletionMenu() => InitializeComponent();

    public void SetItems(IEnumerable<CompletionItem> completions)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (CompletionItem completion in completions)
            {
                ListBoxItem item = new()
                {
                    Content = completion.Name,
                    Tag = completion
                };

                CompletionListBox.Items.Add(item);
            }
        });
    }
}