using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Views;

namespace SkEditor.Controls;
public partial class BottomBarControl : UserControl
{
    public BottomBarControl()
    {
        InitializeComponent();

        Loaded += (sender, e) =>
        {
            Application.Current.ResourcesChanged += (sender, e) => UpdatePosition();
            
            ApiVault.Get().GetTabView().SelectionChanged += (sender, e) => UpdatePosition();
            ApiVault.Get().GetTabView().SelectionChanged += (sender, e) => FileHandler.TabSwitchAction();
        };

        ReloadBottomIcons();
    }

    public void ReloadBottomIcons()
    {
        StackPanel CreatePanel(BottomIconData iconData, Button? button)
        {
            var iconElement = new IconSourceElement();
            var textElement = new TextBlock();
            iconData.Setup(button, textElement, iconElement);

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    iconElement,
                    textElement
                }
            };
        }
        
        IconsStackPanel.Children.Clear();
        var icons = Registries.BottomIcons.ToList();
        icons.Sort((a, b) => a.Order.CompareTo(b.Order));
        
        foreach (var element in icons)
        {
            var button = new Button();

            if (element is BottomIconData bottomIconData)
            {
                button.Content = CreatePanel(bottomIconData, button);
            }
            else
            {
                var group = (BottomIconGroupData) element;
                var elements = new List<(TextBlock, IconSourceElement)>();
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 5
                };
                
                foreach (var child in group.Children)
                {
                    var panel = CreatePanel(child, null);
                    elements.Add(((TextBlock) panel.Children[1], (IconSourceElement) panel.Children[0]));
                    stackPanel.Children.Add(panel);
                }
                
                group.Setup(button);
                button.Content = stackPanel;
            }
            
            IconsStackPanel.Children.Add(button);   
        }
    }

    public void UpdatePosition()
    {
        if (!ApiVault.Get().IsFileOpen()) return;

        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        TextLocation location = textEditor.Document.GetLocation(textEditor.CaretOffset);

        LineText.Text = Translation.Get("BottomBarLine").Replace("{0}", location.Line.ToString());
        ColumnText.Text = Translation.Get("BottomBarColumn").Replace("{0}", location.Column.ToString());
    }

    public void UpdateLogs(string logs)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            LogsText.Text = logs;
        });
    }

    public Grid GetMainGrid() => MainGrid;

    private async void OpenLogsWindow(object? sender, TappedEventArgs e)
    {
        await new LogsWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow());
    }
}
