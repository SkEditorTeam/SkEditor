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
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Views;

namespace SkEditor.Controls;

public partial class BottomBarControl : UserControl
{
    public BottomBarControl()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            Application? current = Application.Current;
            if (current is not null)
            {
                current.ResourcesChanged += (_, _) => UpdatePosition();
            }

            TabView? tabView = SkEditorAPI.Files.GetTabView();
            if (tabView is null)
            {
                return;
            }

            tabView.SelectionChanged += (_, _) => UpdatePosition();
            tabView.SelectionChanged += (_, _) => FileHandler.TabSwitchAction();
        };

        ReloadBottomIcons();
    }

    public void ReloadBottomIcons()
    {
        IconsStackPanel.Children.Clear();
        List<IBottomIconElement> icons = Registries.BottomIcons.ToList();
        icons.Sort((a, b) => a.Order.CompareTo(b.Order));

        foreach (IBottomIconElement element in icons)
        {
            Button button = new();

            if (element is BottomIconData bottomIconData)
            {
                button.Content = CreatePanel(bottomIconData, button);
            }
            else
            {
                BottomIconGroupData group = (BottomIconGroupData)element;
                StackPanel stackPanel = new()
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 5
                };

                foreach (StackPanel panel in group.Children.Select(child => CreatePanel(child, null)))
                {
                    stackPanel.Children.Add(panel);
                }

                group.Setup(button);
                button.Content = stackPanel;
            }

            IconsStackPanel.Children.Add(button);
        }

        return;

        StackPanel CreatePanel(BottomIconData iconData, Button? button)
        {
            IconSourceElement iconElement = new()
            {
                Width = 18,
                Height = 18
            };
            TextBlock textElement = new();
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
    }

    public void UpdatePosition()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
        {
            PositionInfo.IsVisible = false;
            return;
        }

        PositionInfo.IsVisible = true;
        TextEditor? textEditor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (textEditor is null)
        {
            return;
        }

        TextLocation location = textEditor.Document.GetLocation(textEditor.CaretOffset);

        LineText.Text = Translation.Get("BottomBarLine").Replace("{0}", location.Line.ToString());
        ColumnText.Text = Translation.Get("BottomBarColumn").Replace("{0}", location.Column.ToString());
        DocumentSizeText.Text = Translation.Get("BottomBarDocumentSize")
            .Replace("{0}", textEditor.Document.TextLength.ToString());
    }

    public void UpdateLogs(string logs)
    {
        Dispatcher.UIThread.InvokeAsync(() => { LogsText.Text = logs; });
    }

    public Grid GetMainGrid()
    {
        return MainGrid;
    }

    private async void OpenLogsWindow(object? sender, TappedEventArgs e)
    {
        await new LogsWindow().ShowDialogOnMainWindow();
    }
}