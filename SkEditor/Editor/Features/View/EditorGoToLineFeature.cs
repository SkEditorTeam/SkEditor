using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.Editor.Tabs.Capabilities;

namespace SkEditor.Editor.Features.View;

public class EditorGoToLineFeature : IEditorFeature
{
    private TextEditor? _editor;
    private ITextEditorTab? _tab;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not ITextEditorTab textEditorTab)
        {
            return;
        }

        _tab = textEditorTab;
        _editor = editor;

        _tab.GoToLocationRequested += OnGoToLocationRequested;
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        if (_tab != null)
        {
            _tab.GoToLocationRequested -= OnGoToLocationRequested;
        }

        _tab = null;
        _editor = null;
    }

    private void OnGoToLocationRequested(TextLocation location)
    {
        if (_editor == null)
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _editor.ScrollTo(location.Line, location.Column);
            _editor.Focus();
            _editor.CaretOffset = _editor.Document.GetOffset(location);
        });
    }
}