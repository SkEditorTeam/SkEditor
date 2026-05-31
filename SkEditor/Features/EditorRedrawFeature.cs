using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Features;

public class EditorRedrawFeature : IEditorFeature
{
    private TextEditor? _editor;
    private FileTabViewModel? _tab;

    public void OnViewAttached(FileTabViewModel tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        _tab.RedrawRequested += OnRedrawRequested;
    }

    public void OnViewDetached(FileTabViewModel tab, TextEditor editor)
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