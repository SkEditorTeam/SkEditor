using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using SkEditor.API;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Parser;
using SkEditor.Views;
using System;
using System.Linq;

namespace SkEditor.Utilities.Editor;
public class CustomCommandsHandler
{
    public static void OnCommentCommandExecuted(object target)
    {
        OpenedFile file = SkEditorAPI.Files.GetCurrentOpenedFile();
        if (!file.IsEditor) return;

        TextEditor editor = file.Editor;

        var document = editor.Document;
        var selectionStart = editor.SelectionStart;
        var selectionLength = editor.SelectionLength;
        var indentation = editor.Options.IndentationString;

        var selectedLines = document.Lines
            .Where(line => selectionStart <= line.EndOffset && selectionStart + selectionLength >= line.Offset)
            .ToList();

        var modifiedLines = selectedLines.Select(line =>
        {
            var text = document.GetText(line);
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Find the first non-tabulator character
            var strippedLine = text.TrimStart();
            var isCommented = text.TrimStart().StartsWith('#');
            var indentationAmount = 0;
            while (text.StartsWith(indentation))
            {
                text = text[indentation.Length..];
                indentationAmount++;
            }

            string indentationToInsert = "";
            for (int i = 0; i < indentationAmount; i++)
                indentationToInsert += indentation;

            if (isCommented)
            {
                return indentationToInsert + strippedLine[1..];
            }
            else
            {
                return indentationToInsert + "#" + strippedLine;
            }
        }).ToList();

        var replacement = string.Join("\n", modifiedLines);
        var startOffset = selectedLines.First().Offset;
        var endOffset = selectedLines.Last().EndOffset - startOffset;

        document.Replace(startOffset, endOffset, replacement);
        editor.Select(startOffset, replacement.Length);
    }

    public static void OnTrimWhitespacesCommandExecuted(object target)
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
            return;

        TextEditor editor = SkEditorAPI.Files.GetCurrentOpenedFile().Editor;
        var document = editor.Document;
        var selectionStart = editor.SelectionStart;
        var selectionLength = editor.SelectionLength;

        var selectedLines = document.Lines
            .Where(line => selectionStart <= line.EndOffset && selectionStart + selectionLength >= line.Offset)
            .ToList();

        var modifiedLines = selectedLines.Select(line =>
        {
            var text = document.GetText(line);
            if (string.IsNullOrWhiteSpace(text))
                return "";
            return text;
        }).ToList();

        var replacement = string.Join("\n", modifiedLines);
        var startOffset = selectedLines.First().Offset;
        var endOffset = selectedLines.Last().EndOffset - startOffset;

        document.Replace(startOffset, endOffset, replacement);
        editor.Select(startOffset, replacement.Length);
    }

    public static void OnDuplicateCommandExecuted(object target)
    {
        if (target is not TextArea textArea || textArea.Document == null) return;

        if (textArea.Selection.IsEmpty)
        {
            DocumentLine caretLine = textArea.Document.GetLineByNumber(textArea.Caret.Line);
            string lineText = textArea.Document.GetText(caretLine.Offset, caretLine.Length);
            textArea.Document.Insert(caretLine.EndOffset, Environment.NewLine + lineText);
            textArea.Caret.BringCaretToView();
            return;
        }

        string selectedText = textArea.Selection.GetText();

        int endOffset = textArea.Document.GetLineByNumber(textArea.Selection.EndPosition.Line).Offset + textArea.Document.GetLineByNumber(textArea.Selection.EndPosition.Line).Length;
        int startOffset = textArea.Document.GetLineByNumber(textArea.Selection.StartPosition.Line).Offset + textArea.Document.GetLineByNumber(textArea.Selection.StartPosition.Line).Length;

        int usedOffset = Math.Max(startOffset, endOffset);

        textArea.Document.Insert(usedOffset, $"{Environment.NewLine}{selectedText}");

        int newCaretOffset = usedOffset + Environment.NewLine.Length + selectedText.Length;
        textArea.Caret.Offset = newCaretOffset;
        textArea.Caret.BringCaretToView();
    }

    public static async void OnRefactorCommandExecuted(TextEditor editor)
    {
        var parser = SkEditorAPI.Files.GetOpenedFiles().Find(file => file.Editor == editor).Parser;
        if (parser == null)
            return;
        if (!parser.IsParsed)
            parser.Parse();

        var section = parser.GetSectionFromLine(editor.TextArea.Caret.Line);
        if (section == null)
            return;

        var variable = section.GetVariableFromCaret(editor.TextArea.Caret);
        var option = section.GetOptionFromCaret(editor.TextArea.Caret);
        if (variable == null && option == null)
            return;

        var renameWindow = new SymbolRefactorWindow((INameableCodeElement)variable ?? option);
        await renameWindow.ShowDialog(SkEditorAPI.Windows.GetMainWindow());
    }
}
