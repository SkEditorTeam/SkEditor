using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using SkEditor.Utilities.Extensions;
using LogsWindow = SkEditor.Views.Windows.LogsWindow;

namespace SkEditor.Views.Controls;

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

            // TabView? tabView = SkEditorAPI.Files.GetTabView();
            // if (tabView is null)
            // {
            //     return;
            // }
            //
            // tabView.SelectionChanged += (_, _) => UpdatePosition();
            // tabView.SelectionChanged += (_, _) => FileHandler.TabSwitchAction();
        };

        ReloadBottomIcons();
    }

    public void ReloadBottomIcons()
    {
        IconsStackPanel.Children.Clear();
    }

    public void UpdatePosition()
    {
        // if (!SkEditorAPI.Files.IsEditorOpen())
        // {
        //     PositionInfo.IsVisible = false;
        //     return;
        // }
        //
        // PositionInfo.IsVisible = true;
        // TextEditor? textEditor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        // if (textEditor is null)
        // {
        //     return;
        // }
        //
        // TextLocation location = textEditor.Document.GetLocation(textEditor.CaretOffset);
        //
        // LineText.Text = Translation.Get(TranslationKey.BottomBarLine).Replace("{0}", location.Line.ToString());
        // ColumnText.Text = Translation.Get(TranslationKey.BottomBarColumn).Replace("{0}", location.Column.ToString());
        // DocumentSizeText.Text = Translation.Get(TranslationKey.BottomBarDocumentSize)
        //     .Replace("{0}", textEditor.Document.TextLength.ToString());
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