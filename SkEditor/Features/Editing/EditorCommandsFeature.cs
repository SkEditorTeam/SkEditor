using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Editor.Commands;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features.Editing;

public class EditorCommandsFeature(IEnumerable<IEditorCommand> commands) : IEditorFeature
{
    private TextEditor? _editor;
    private ITextEditorTab? _tab;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not ITextEditorTab editorTab)
        {
            return;
        }

        _tab = editorTab;
        _editor = editor;

        editorTab.EditorCommandRequested += OnCommandRequested;
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        if (_tab is not null)
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