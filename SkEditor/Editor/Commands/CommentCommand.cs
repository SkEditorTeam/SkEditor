using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace SkEditor.Editor.Commands;

public class CommentCommand : IEditorCommand
{
    public EditorCommandType CommandType => EditorCommandType.Comment;

    public void Execute(TextDocument document, TextEditor editor)
    {
        int selectionStart = editor.SelectionStart;
        int selectionLength = editor.SelectionLength;
        string? indentation = editor.Options.IndentationString;

        List<DocumentLine> selectedLines = document.Lines
            .Where(line => selectionStart <= line.EndOffset && selectionStart + selectionLength >= line.Offset)
            .ToList();

        List<string> modifiedLines = selectedLines.Select(line =>
        {
            string? text = document.GetText(line);
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // Find the first non-tabulator character
            string strippedLine = text.TrimStart();
            bool isCommented = text.TrimStart().StartsWith('#');
            int indentationAmount = 0;
            while (text.StartsWith(indentation))
            {
                text = text[indentation.Length..];
                indentationAmount++;
            }

            string indentationToInsert = "";
            for (int i = 0; i < indentationAmount; i++)
            {
                indentationToInsert += indentation;
            }

            if (isCommented)
            {
                return indentationToInsert + strippedLine[1..];
            }

            return indentationToInsert + "#" + strippedLine;
        }).ToList();

        string replacement = string.Join("\n", modifiedLines);
        int startOffset = selectedLines.First().Offset;
        int endOffset = selectedLines.Last().EndOffset - startOffset;

        document.Replace(startOffset, endOffset, replacement);
        editor.Select(startOffset, replacement.Length);
    }
}