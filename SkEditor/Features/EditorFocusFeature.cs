using AvaloniaEdit;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Features;

public class EditorFocusFeature : IEditorFeature
{
    private TextEditor? _editor;
    private FileTabViewModel? _tab;

    public void OnViewAttached(FileTabViewModel tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        _tab.FocusRequested += OnFocusRequested;
    }

    public void OnViewDetached(FileTabViewModel tab, TextEditor editor)
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