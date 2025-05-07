using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Parser;
using SkEditor.Views;

namespace SkEditor.Utilities.Editor;

public class CustomCommandsHandler
{
    public static void OnCommentCommandExecuted(object target)
    {
        OpenedFile? file = SkEditorAPI.Files.GetCurrentOpenedFile();
        if (file is not { Editor: { } editor })
        {
            return;
        }

        TextDocument? document = editor.Document;
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

    public static void OnTrimWhitespacesCommandExecuted(object target)
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
        {
            return;
        }

        TextEditor? editor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (editor == null) return;
        TextDocument? document = editor.Document;
        int selectionStart = editor.SelectionStart;
        int selectionLength = editor.SelectionLength;

        List<DocumentLine> selectedLines = document.Lines
            .Where(line => selectionStart <= line.EndOffset && selectionStart + selectionLength >= line.Offset)
            .ToList();

        List<string> modifiedLines = selectedLines.Select(line =>
        {
            string? text = document.GetText(line);
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            return text;
        }).ToList();

        string replacement = string.Join("\n", modifiedLines);
        int startOffset = selectedLines.First().Offset;
        int endOffset = selectedLines.Last().EndOffset - startOffset;

        document.Replace(startOffset, endOffset, replacement);
        editor.Select(startOffset, replacement.Length);
    }

    public static void OnDuplicateCommandExecuted(object target)
    {
        if (target is not TextArea textArea || textArea.Document == null)
        {
            return;
        }

        if (textArea.Selection.IsEmpty)
        {
            DocumentLine caretLine = textArea.Document.GetLineByNumber(textArea.Caret.Line);
            string lineText = textArea.Document.GetText(caretLine.Offset, caretLine.Length);
            textArea.Document.Insert(caretLine.EndOffset, Environment.NewLine + lineText);
            textArea.Caret.BringCaretToView();
            return;
        }

        string selectedText = textArea.Selection.GetText();

        int endOffset = textArea.Document.GetLineByNumber(textArea.Selection.EndPosition.Line).Offset +
                        textArea.Document.GetLineByNumber(textArea.Selection.EndPosition.Line).Length;
        int startOffset = textArea.Document.GetLineByNumber(textArea.Selection.StartPosition.Line).Offset +
                          textArea.Document.GetLineByNumber(textArea.Selection.StartPosition.Line).Length;

        int usedOffset = Math.Max(startOffset, endOffset);

        textArea.Document.Insert(usedOffset, $"{Environment.NewLine}{selectedText}");

        int newCaretOffset = usedOffset + Environment.NewLine.Length + selectedText.Length;
        textArea.Caret.Offset = newCaretOffset;
        textArea.Caret.BringCaretToView();
    }

    public static async Task OnRefactorCommandExecuted(TextEditor editor)
    {
        CodeParser? parser = SkEditorAPI.Files.GetOpenedFiles().Find(file => file.Editor == editor)?.Parser;
        if (parser == null)
        {
            return;
        }

        if (!parser.IsParsed)
        {
            parser.Parse();
        }

        CodeSection? section = parser.GetSectionFromLine(editor.TextArea.Caret.Line);
        if (section == null)
        {
            return;
        }

        CodeVariable? variable = section.GetVariableFromCaret(editor.TextArea.Caret);
        CodeOption? option = section.GetOptionFromCaret(editor.TextArea.Caret);
        if (variable == null && option == null)
        {
            return;
        }
        
        INameableCodeElement? nameableElement = (INameableCodeElement?)variable ?? option;
        if (nameableElement == null) return;

        SymbolRefactorWindow renameWindow = new(nameableElement);
        await renameWindow.ShowDialogOnMainWindow();
    }
}