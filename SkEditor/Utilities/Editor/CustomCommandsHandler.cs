using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using SkEditor.API;
using System;
using System.Linq;

namespace SkEditor.Utilities.Editor;
public class CustomCommandsHandler
{
	public static void OnCommentCommandExecuted(object target)
	{
		TextEditor editor = ApiVault.Get().GetTextEditor();

		var document = editor.Document;
		var selectionStart = editor.SelectionStart;
		var selectionLength = editor.SelectionLength;

		var selectedLines = document.Lines
			.Where(line => selectionStart <= line.EndOffset && selectionStart + selectionLength >= line.Offset)
			.ToList();

		bool allLinesCommented = selectedLines.All(line => document.GetText(line).StartsWith("#"));

		var modifiedLines = selectedLines.Select(line =>
		{
			var text = document.GetText(line);
			if (allLinesCommented)
			{
				return text.StartsWith('#') ? text[1..] : text;
			}
			else
			{
				return text.StartsWith('#') ? "##" + text[1..] : "#" + text;
			}
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
			textArea.Caret.Line++;
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
}
