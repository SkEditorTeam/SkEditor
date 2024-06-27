using Avalonia.Controls;
using Avalonia.Threading;
using SkEditor.Utilities.Completion;
using System.Collections.Generic;
using Avalonia.Layout;
using Avalonia.Media;

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
                var grid = new Grid()
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    RowDefinitions = new RowDefinitions("*,*")
                };

                var nameBox = new TextBlock()
                {
                    Text = completion.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 14,
                    FontWeight = FontWeight.SemiBold
                };

                if (completion.Description != null)
                {
                    var desc = new TextBlock()
                    {
                        Text = completion.Description,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        FontSize = 12,
                        Foreground = Brushes.Gray
                    };
                    
                    grid.Children.Add(nameBox);
                    grid.Children.Add(desc);
                    
                    Grid.SetColumn(nameBox, 0);
                    Grid.SetRow(nameBox, 0);
                    
                    Grid.SetColumn(desc, 0);
                    Grid.SetRow(desc, 1);
                }
                else
                {
                    grid.Children.Add(nameBox);
                    
                    Grid.SetColumn(nameBox, 0);
                    Grid.SetRow(nameBox, 0);
                    Grid.SetRowSpan(nameBox, 2);
                }
                 
                ListBoxItem item = new()
                {
                    Content = grid,
                    Tag = completion
                };

                CompletionListBox.Items.Add(item);
            }
        });
    }
}
