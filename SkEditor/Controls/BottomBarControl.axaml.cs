using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;

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
    }

    public void UpdatePosition()
    {
        if (!ApiVault.Get().IsFileOpen()) return;

        TextEditor textEditor = ApiVault.Get().GetTextEditor();
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
}
