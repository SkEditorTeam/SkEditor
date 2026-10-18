using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace SkEditor.Editor.Commands;

public interface IEditorCommand
{
    EditorCommandType CommandType { get; }

    void Execute(TextDocument document, TextEditor editor);
}