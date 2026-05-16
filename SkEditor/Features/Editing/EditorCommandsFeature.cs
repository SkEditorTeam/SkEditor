using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Editor.Commands;

namespace SkEditor.Features.Editing;

public class EditorCommandsFeature(IEnumerable<IEditorCommand> commands) : IEditorFeature
{
    private TextEditor? _editor;
    private FileTab? _tab;

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        _tab.EditorCommandRequested += OnCommandRequested;
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        if (_tab != null)
        {
            _tab.EditorCommandRequested -= OnCommandRequested;
        }

        _tab = null;
        _editor = null;
    }

    private void OnCommandRequested(EditorCommandType commandType)
    {
        if (_editor == null || _tab == null)
        {
            return;
        }

        switch (commandType)
        {
            case EditorCommandType.Copy:
                _editor.Copy();
                break;
            case EditorCommandType.Paste:
                _editor.Paste();
                break;
            case EditorCommandType.Cut:
                _editor.Cut();
                break;
            case EditorCommandType.Undo:
                _editor.Undo();
                break;
            case EditorCommandType.Redo:
                _editor.Redo();
                break;
            case EditorCommandType.Find:
                _editor.SearchPanel.Open();
                break;
            case EditorCommandType.Replace:
                _editor.SearchPanel.IsReplaceMode = true;
                _editor.SearchPanel.Open();
                break;

            default:
                commands.FirstOrDefault(c => c.CommandType == commandType)?.Execute(_editor.Document, _editor);
                break;
        }
    }
}