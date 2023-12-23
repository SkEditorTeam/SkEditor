using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.API;
using SkEditor.Utilities;

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
        };
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
}
