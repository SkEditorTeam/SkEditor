using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public class EditorActionsFeature : IEditorFeature
{
    private TextEditor? _editor;
    private FileTab? _tab;

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        _tab.RedrawRequested += OnRedrawRequested;
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        if (_tab != null)
        {
            _tab.RedrawRequested -= OnRedrawRequested;
        }

        _tab = null;
        _editor = null;
    }

    private void OnRedrawRequested(ISegment? segment)
    {
        if (segment is not null)
        {
            _editor?.TextArea.TextView.Redraw(segment);
        }
        else
        {
            _editor?.TextArea.TextView.Redraw();
        }
    }
}