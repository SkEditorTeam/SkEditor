using System;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace SkEditor.Editor.Commands;

public class DuplicateCommand : IEditorCommand
{
    public EditorCommandType CommandType => EditorCommandType.Duplicate;

    public void Execute(TextDocument document, TextEditor editor)
    {
        TextArea textArea = editor.TextArea;

        if (editor.SelectionLength == 0)
        {
            DocumentLine caretLine = document.GetLineByNumber(textArea.Caret.Line);
            string lineText = document.GetText(caretLine.Offset, caretLine.Length);
            document.Insert(caretLine.EndOffset, Environment.NewLine + lineText);
            textArea.Caret.BringCaretToView();
            return;
        }

        string selectedText = textArea.Selection.GetText();

        int endOffset = document.GetLineByNumber(textArea.Selection.EndPosition.Line).Offset +
                        document.GetLineByNumber(textArea.Selection.EndPosition.Line).Length;
        int startOffset = document.GetLineByNumber(textArea.Selection.StartPosition.Line).Offset +
                          document.GetLineByNumber(textArea.Selection.StartPosition.Line).Length;

        int usedOffset = Math.Max(startOffset, endOffset);

        document.Insert(usedOffset, $"{Environment.NewLine}{selectedText}");

        int newCaretOffset = usedOffset + Environment.NewLine.Length + selectedText.Length;
        textArea.Caret.Offset = newCaretOffset;
        textArea.Caret.BringCaretToView();
    }
}