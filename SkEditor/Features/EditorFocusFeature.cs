using AvaloniaEdit;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public class EditorFocusFeature : IEditorFeature
{
    private TextEditor? _editor;
    private FileTab? _tab;

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        _tab.FocusRequested += OnFocusRequested;
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        if (_tab != null)
        {
            _tab.FocusRequested -= OnFocusRequested;
        }

        _tab = null;
        _editor = null;
    }

    private void OnFocusRequested()
    {
        _editor?.Focus();
    }
}