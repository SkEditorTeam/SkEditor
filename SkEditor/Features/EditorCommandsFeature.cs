using AvaloniaEdit;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Editor;

namespace SkEditor.Features;

public class EditorCommandsFeature : IEditorFeature
{
    private TextEditor? _editor;
    private FileTab? _tab;

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        _tab.EditorActionRequested += OnActionRequested;
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        if (_tab != null)
        {
            _tab.EditorActionRequested -= OnActionRequested;
        }

        _tab = null;
        _editor = null;
    }

    private void OnActionRequested(EditorActionType actionType)
    {
        if (_editor == null)
        {
            return;
        }

        switch (actionType)
        {
            case EditorActionType.Copy:
                _editor.Copy();
                break;
            case EditorActionType.Paste:
                _editor.Paste();
                break;
            case EditorActionType.Cut:
                _editor.Cut();
                break;
            case EditorActionType.Undo:
                _editor.Undo();
                break;
            case EditorActionType.Redo:
                _editor.Redo();
                break;
            case EditorActionType.Duplicate:
                // TODO
                break;
            case EditorActionType.Comment:
                // TODO
                break;
            case EditorActionType.TrimWhitespace:
                // TODO
                break;
            case EditorActionType.Find:
                _editor.SearchPanel.Open();
                break;
            case EditorActionType.Replace:
                _editor.SearchPanel.IsReplaceMode = true;
                _editor.SearchPanel.Open();
                break;
        }
    }
}