using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace SkEditor.Utilities.Editor.Commands;

public interface IEditorCommand
{
    EditorActionType ActionType { get; }

    void Execute(TextDocument document, TextEditor editor);
}