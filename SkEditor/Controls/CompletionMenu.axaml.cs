using Avalonia.Controls;
using Avalonia.Threading;
using SkEditor.Utilities.Completion;
using System.Collections.Generic;

namespace SkEditor.Controls;
public partial class CompletionMenu : UserControl
{
    public CompletionMenu(IEnumerable<CompletionItem> completions)
    {
        InitializeComponent();

        SetItems(completions);
    }

    public void SetItems(IEnumerable<CompletionItem> completions)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var completion in completions)
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
