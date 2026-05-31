using AvaloniaEdit;

namespace SkEditor.Features;

public class EditorFocusFeature : IEditorFeature
{
    private TextEditor? _editor;
    private IFeatureTab? _tab;

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        _tab.FocusRequested += OnFocusRequested;
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
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
        _editor?.TextArea.Focus();
    }
}