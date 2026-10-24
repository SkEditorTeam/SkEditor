using System;
using System.Collections.Generic;
using Avalonia;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;

namespace SkEditor.Editor;

public static class TextEditorUtilities
{
    public static SimpleSegment GetWordAtMousePosition(Point pos, TextArea textArea)
    {
        TextView? textView = textArea.TextView;
        if (textView == null)
        {
            return SimpleSegment.Invalid;
        }

        if (pos.Y < 0)
        {
            pos = pos.WithY(0);
        }

        if (pos.Y > textView.Bounds.Height)
        {
            pos = pos.WithY(textView.Bounds.Height);
        }

        pos += textView.ScrollOffset;

        VisualLine? line = textView.GetVisualLineFromVisualTop(pos.Y);

        if (line?.TextLines == null)
        {
            return SimpleSegment.Invalid;
        }

        int visualColumn = line.GetVisualColumn(pos, textArea.Selection.EnableVirtualSpace);

        int wordStartVc = line.GetNextCaretPosition(visualColumn + 1, LogicalDirection.Backward,
            CaretPositioningMode.WordStartOrSymbol, textArea.Selection.EnableVirtualSpace);
        if (wordStartVc == -1)
        {
            wordStartVc = 0;
        }

        int wordEndVc = line.GetNextCaretPosition(wordStartVc, LogicalDirection.Forward,
            CaretPositioningMode.WordBorderOrSymbol, textArea.Selection.EnableVirtualSpace);
        if (wordEndVc == -1)
        {
            wordEndVc = line.VisualLength;
        }

        int relOffset = line.FirstDocumentLine.Offset;
        int wordStartOffset = line.GetRelativeOffset(wordStartVc) + relOffset;
        int wordEndOffset = line.GetRelativeOffset(wordEndVc) + relOffset;

        return new SimpleSegment(wordStartOffset, wordEndOffset - wordStartOffset);
    }

    public static SimpleSegment GetSegmentBeforeOffset(int offset, TextDocument document)
    {
        if (offset <= 0)
        {
            return SimpleSegment.Invalid;
        }

        DocumentLine? line = document.GetLineByOffset(offset);
        if (line == null)
        {
            return SimpleSegment.Invalid;
        }

        string? lineText = document.GetText(line.Offset, offset - line.Offset);
        int wordStartPos = lineText.LastIndexOfAny([' ', '\t', '\n', '\r']);
        return wordStartPos == -1
            ? new SimpleSegment(line.Offset, offset - line.Offset)
            : new SimpleSegment(line.Offset + wordStartPos + 1, offset - line.Offset - wordStartPos - 1);
    }

    public static IEnumerable<SimpleSegment> GetWordOccurrences(string word, TextDocument document)
    {
        string wholeText = document.Text;
        int offset = 0;
        int wordStartPos;

        while ((wordStartPos = wholeText.IndexOf(word, offset, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            yield return new SimpleSegment(wordStartPos, word.Length);
            offset = wordStartPos + 1;
        }
    }

    public static void DeletePreviousWord(TextEditor editor)
    {
        TextDocument document = editor.Document;

        int caretOffset = editor.CaretOffset;
        DocumentLine currentLine = document.GetLineByOffset(caretOffset);
        int lineStartOffset = currentLine.Offset;
        int lengthToCaret = caretOffset - lineStartOffset;

        if (lengthToCaret > 0)
        {
            ReadOnlySpan<char> textToCaret =
                document.GetTextAsMemory(lineStartOffset, lengthToCaret).Span;

            if (textToCaret.TrimStart().IsEmpty)
            {
                document.Remove(lineStartOffset, lengthToCaret);
                return;
            }
        }

        int wordStart = TextUtilities.GetNextCaretPosition(
            document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

        if (wordStart >= 0)
        {
            document.Remove(wordStart, caretOffset - wordStart);
        }
    }


    public static void MoveSelectionUp(TextEditor editor)
    {
        TextDocument document = editor.Document;
        (DocumentLine startLine, DocumentLine endLine) = GetSelectedLineRange(editor);

        DocumentLine? previousLine = startLine.PreviousLine;
        if (previousLine is null)
        {
            return;
        }

        int start = editor.SelectionStart;
        int length = editor.SelectionLength;
        int caretOffset = editor.CaretOffset;

        string selectedLines = document.GetText(
            startLine.Offset, endLine.EndOffset - startLine.Offset);
        string delimiter = document.GetText(
            previousLine.EndOffset, previousLine.DelimiterLength);
        string previousText = document.GetText(
            previousLine.Offset, previousLine.Length);

        int offset = previousLine.Offset;
        int shift = previousLine.TotalLength;

        using (document.RunUpdate())
        {
            document.Replace(
                offset,
                endLine.EndOffset - offset,
                selectedLines + delimiter + previousText);

            editor.Select(start - shift, length);
            editor.CaretOffset = caretOffset - shift;
        }
    }

    public static void MoveSelectionDown(TextEditor editor)
    {
        TextDocument document = editor.Document;
        (DocumentLine startLine, DocumentLine endLine) = GetSelectedLineRange(editor);

        DocumentLine? nextLine = endLine.NextLine;
        if (nextLine is null)
        {
            return;
        }

        int start = editor.SelectionStart;
        int length = editor.SelectionLength;
        int caretOffset = editor.CaretOffset;

        string selectedLines = document.GetText(
            startLine.Offset, endLine.EndOffset - startLine.Offset);
        string delimiter = document.GetText(
            endLine.EndOffset, endLine.DelimiterLength);
        string nextText = document.GetText(
            nextLine.Offset, nextLine.Length);

        int shift = nextLine.Length + endLine.DelimiterLength;
        int newStart = start + shift;

        using (document.RunUpdate())
        {
            document.Replace(
                startLine.Offset,
                nextLine.EndOffset - startLine.Offset,
                nextText + delimiter + selectedLines);

            int newLength = Math.Min(length, document.TextLength - newStart);
            editor.Select(newStart, newLength);
            editor.CaretOffset = Math.Min(caretOffset + shift, document.TextLength);
        }
    }

    private static (DocumentLine StartLine, DocumentLine EndLine) GetSelectedLineRange(TextEditor editor)
    {
        TextDocument document = editor.Document;
        int start = editor.SelectionStart;
        int length = editor.SelectionLength;
        int end = start + length;

        DocumentLine startLine = document.GetLineByOffset(start);
        DocumentLine endLine = document.GetLineByOffset(length > 0 ? end - 1 : end);

        return (startLine, endLine);
    }
}