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
using System.Linq;

namespace SkEditor.Controls;
public partial class BottomBarControl : UserControl
{
    public BottomBarControl()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            Application.Current.ResourcesChanged += (_, _) => UpdatePosition();

            SkEditorAPI.Files.GetTabView().SelectionChanged += (_, _) => UpdatePosition();
            SkEditorAPI.Files.GetTabView().SelectionChanged += (_, _) => FileHandler.TabSwitchAction();
        };

        ReloadBottomIcons();
    }

    public void ReloadBottomIcons()
    {
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
                var group = (BottomIconGroupData)element;
                var stackPanel = new StackPanel
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
            var iconElement = new IconSourceElement
            {
                Width = 18,
                Height = 18
            };
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
    }

    public void UpdatePosition()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
        {
            PositionInfo.IsVisible = false;
            return;
        }

        PositionInfo.IsVisible = true;
        TextEditor textEditor = SkEditorAPI.Files.GetCurrentOpenedFile().Editor;
        TextLocation location = textEditor.Document.GetLocation(textEditor.CaretOffset);

        LineText.Text = Translation.Get("BottomBarLine").Replace("{0}", location.Line.ToString());
        ColumnText.Text = Translation.Get("BottomBarColumn").Replace("{0}", location.Column.ToString());
        DocumentSizeText.Text = Translation.Get("BottomBarDocumentSize").Replace("{0}", textEditor.Document.TextLength.ToString());
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
