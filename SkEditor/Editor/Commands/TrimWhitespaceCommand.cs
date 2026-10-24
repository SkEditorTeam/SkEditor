using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace SkEditor.Editor.Commands;

public class TrimWhitespaceCommand : IEditorCommandHandler
{
    public EditorCommandType[] HandledCommandTypes => [EditorCommandType.TrimWhitespace];

    public void Execute(TextDocument document, TextEditor editor, EditorCommandType _)
    {
        List<DocumentLine> selectedLines;
        int currentSelectionStart = editor.SelectionStart;
        int currentSelectionLength = editor.SelectionLength;

        if (editor.SelectionLength > 0)
        {
            int selectionStart = editor.SelectionStart;
            int selectionLength = editor.SelectionLength;

            selectedLines = document.Lines
                .Where(line => selectionStart <= line.EndOffset && selectionStart + selectionLength >= line.Offset)
                .ToList();
        }
        else
        {
            selectedLines = document.Lines.ToList();
        }

        if (selectedLines.Count == 0)
        {
            return;
        }

        TextAnchor? startAnchor = document.CreateAnchor(currentSelectionStart);
        startAnchor.SurviveDeletion = true;
        startAnchor.MovementType = AnchorMovementType.BeforeInsertion;

        TextAnchor? endAnchor = document.CreateAnchor(currentSelectionStart + currentSelectionLength);
        endAnchor.SurviveDeletion = true;
        endAnchor.MovementType = AnchorMovementType.BeforeInsertion;

        using (document.RunUpdate())
        {
            for (int i = selectedLines.Count - 1; i >= 0; i--)
            {
                DocumentLine line = selectedLines[i];
                ReadOnlyMemory<char> text = document.GetTextAsMemory(line.Offset, line.Length);

                ReadOnlyMemory<char> trimmedText = text.TrimEnd();

                int whitespaceLength = text.Length - trimmedText.Length;
                if (whitespaceLength > 0)
                {
                    document.Remove(line.Offset + trimmedText.Length, whitespaceLength);
                }
            }

            if (startAnchor.IsDeleted || endAnchor.IsDeleted)
            {
                return;
            }

            int newStart = startAnchor.Offset;
            int newEnd = endAnchor.Offset;

            newStart = Math.Clamp(newStart, 0, document.TextLength);
            newEnd = Math.Clamp(newEnd, newStart, document.TextLength);

            editor.Select(newStart, newEnd - newStart);
        }
    }
}