using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace SkEditor.Editor.Commands;

public interface IEditorCommandHandler
{
    EditorCommandType[] HandledCommandTypes { get; }

    void Execute(TextDocument document, TextEditor editor, EditorCommandType commandType);
}