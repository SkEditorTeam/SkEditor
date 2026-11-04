using System;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace SkEditor.Editor.Commands;

public class DuplicateCommand : IEditorCommandHandler
{
    public EditorCommandType[] HandledCommandTypes => [EditorCommandType.Duplicate];
    
    public void Execute(TextDocument document, TextEditor editor, EditorCommandType _)
    {
        TextArea textArea = editor.TextArea;

        if (editor.SelectionLength == 0)
        {
            int targetColumn = textArea.Caret.Column;
            int targetLine = textArea.Caret.Line;

            DocumentLine caretLine = document.GetLineByNumber(targetLine);
            string lineText = document.GetText(caretLine.Offset, caretLine.Length);
            string newLine = TextUtilities.GetNewLineFromDocument(document, targetLine);

            document.Insert(caretLine.EndOffset, newLine + lineText);
            
            textArea.Caret.Location = new TextLocation(targetLine + 1, targetColumn);
            textArea.Caret.BringCaretToView();
            
            return;
        }

        string selectedText = textArea.Selection.GetText();

        int endOffset = document.GetLineByNumber(textArea.Selection.EndPosition.Line).Offset +
                        document.GetLineByNumber(textArea.Selection.EndPosition.Line).Length;
        int startOffset = document.GetLineByNumber(textArea.Selection.StartPosition.Line).Offset +
                          document.GetLineByNumber(textArea.Selection.StartPosition.Line).Length;

        int usedOffset = Math.Max(startOffset, endOffset);

        document.Insert(usedOffset, selectedText);

        int caretOffset = usedOffset + selectedText.Length;
        textArea.Caret.Offset = caretOffset;
        textArea.Caret.BringCaretToView();
        editor.Select(caretOffset - selectedText.Length, selectedText.Length);
    }
}