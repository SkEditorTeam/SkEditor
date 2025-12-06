using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using SkEditor.API;
using SkEditor.Utilities.Files;

namespace SkEditor.Utilities.Editor;

public partial class TextEditorEventHandler
{
    private const string CommentPattern = @"#(?!#(?:\s*#[^#]*)?)\s*[^#]*$";

    private static readonly Dictionary<char, char> SymbolPairs = new()
    {
        { '(', ')' },
        { '[', ']' },
        { '"', '"' },
        { '<', '>' },
        { '{', '}' },
        { '%', '%' }
    };

    public static Dictionary<TextEditor, ScrollViewer> ScrollViewers { get; } = [];

    public static void OnZoom(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier())
        {
            return;
        }

        e.Handled = true;

        TextEditor? editor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (editor == null)
        {
            return;
        }

        int zoom = e.Delta.Y > 0 && editor.FontSize < 200 ? 1 : -1;

        Zoom(editor, zoom);
    }

    public static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier())
        {
            return;
        }

        TextEditor? editor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (editor == null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.OemPlus:
                Zoom(editor, 5);
                break;
            case Key.OemMinus:
                Zoom(editor, -5);
                break;
        }
    }

    private static void Zoom(TextEditor editor, int value)
    {
        if (SkEditorAPI.Core.GetAppConfig().IsZoomSyncEnabled)
        {
            foreach (OpenedFile openedFile in SkEditorAPI.Files.GetOpenedEditors())
            {
                ZoomEditor(value, openedFile.Editor!);
            }
        }
        else
        {
            ZoomEditor(value, editor);
        }
    }

    public static void ZoomEditor(int value, TextEditor editor)
    {
        if (value < 0 && editor.FontSize <= 5)
        {
            return;
        }

        double oldLineHeight = editor.TextArea.TextView.DefaultLineHeight;
        editor.FontSize += value;
        double lineHeight = editor.TextArea.TextView.DefaultLineHeight;

        double lineHeightChange = lineHeight / oldLineHeight;

        ScrollViewer? scrollViewer = editor.ScrollViewer;
        if (scrollViewer == null)
        {
            return;
        }

        double newOffset = scrollViewer.Offset.Y * lineHeightChange;

        scrollViewer.SetCurrentValue(ScrollViewer.OffsetProperty, new Vector(scrollViewer.Offset.X, newOffset));
    }

    public static async void OnTextChanged(object? sender, EventArgs e)
    {
        try
        {
            if (!SkEditorAPI.Files.IsEditorOpen())
            {
                return;
            }

            TextEditor? editor = sender as TextEditor;
            OpenedFile? openedFile = SkEditorAPI.Files.GetOpenedFiles().Find(tab => tab.Editor == editor);
            if (openedFile == null)
            {
                return;
            }

            if (SkEditorAPI.Core.GetAppConfig().IsAutoSaveEnabled && !string.IsNullOrEmpty(openedFile.Path))
            {
                openedFile.IsSaved = false;
                await Dispatcher.UIThread.InvokeAsync(FileHandler.SaveFile);
                return;
            }

            if (SkEditorAPI.Core.GetAppConfig().EnableRealtimeCodeParser)
            {
                await Dispatcher.UIThread.InvokeAsync(() => openedFile.Parser?.Parse());
            }

            openedFile.IsSaved = false;
            openedFile.IsNewFile = false;
        }
        catch (Exception exc)
        {
            SkEditorAPI.Logs.Error($"Error in TextEditorEventHandler.OnTextChanged: {exc}");
        }
    }

    public static void DoAutoIndent(object? sender, TextInputEventArgs e)
    {
        if (!SkEditorAPI.Core.GetAppConfig().IsAutoIndentEnabled)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(e.Text))
        {
            return;
        }

        TextEditor? textEditor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (textEditor == null) return;

        DocumentLine line = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
        if (!string.IsNullOrWhiteSpace(textEditor.Document.GetText(line)))
        {
            return;
        }

        if (line.PreviousLine == null)
        {
            return;
        }

        DocumentLine previousLine = line.PreviousLine;

        string previousLineText = textEditor.Document.GetText(previousLine);
        previousLineText = CommentRegex().Replace(previousLineText, "");
        previousLineText = previousLineText.TrimEnd();

        if (!previousLineText.EndsWith(':'))
        {
            return;
        }

        textEditor.Document.Insert(line.Offset, textEditor.Options.IndentationString);
    }

    public static void DoAutoPairing(object? sender, TextInputEventArgs e)
    {
        if (!SkEditorAPI.Core.GetAppConfig().IsAutoPairingEnabled)
        {
            return;
        }

        char? symbol = e.Text?[0];
        if (symbol == null) return;
        
        if (!SymbolPairs.TryGetValue(symbol.Value, out char value))
        {
            return;
        }

        TextEditor? textEditor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (textEditor == null) return;
        
        if (textEditor.Document.TextLength > textEditor.CaretOffset)
        {
            string nextChar = textEditor.Document.GetText(textEditor.CaretOffset, 1);
            if (nextChar.Equals(value.ToString()))
            {
                return;
            }
        }

        int lineOffset = textEditor.Document.GetLineByOffset(textEditor.CaretOffset).Offset;
        string textBefore = textEditor.Document.GetText(lineOffset, textEditor.CaretOffset - lineOffset - 1);
        int count1 = textBefore.Count(c => c == symbol);
        int count2 = textBefore.Count(c => c == value);
        if (symbol == value && count1 % 2 == 1)
        {
            return;
        }

        if (count1 > count2)
        {
            return;
        }

        textEditor.Document.Insert(textEditor.CaretOffset, value.ToString());
        textEditor.CaretOffset--;
    }

    public static void CheckForHex(TextEditor textEditor)
    {
        Regex regex = HexRegex();

        Dispatcher.UIThread.Post(() =>
        {
            MatchCollection matches = regex.Matches(textEditor.Text);

            if (textEditor.SyntaxHighlighting == null)
            {
                return;
            }

            HighlightingRuleSet ruleSet =
                textEditor.SyntaxHighlighting.GetNamedRuleSet("BracedExpressionAndColorsRuleSet");
            if (ruleSet == null)
            {
                return;
            }

            foreach (Match match in matches.Cast<Match>())
            {
                string hex = match.Value.Contains("color:#") ? match.Value[7..^1] : match.Value.Contains("##") ? match.Value[2..^1] : match.Value[1..^1];
                bool parsed = Color.TryParse(hex, out Color color);
                if (!parsed)
                {
                    continue;
                }

                if (ruleSet.Spans.Any(s => s != null && s.StartExpression.ToString().Contains(hex)))
                {
                    textEditor.TextArea.TextView.Redraw(match.Index, match.Length);
                    continue;
                }

                HighlightingSpan span = new()
                {
                    StartExpression = new Regex("<(color:|#)?" + hex + ">"),
                    EndExpression = EmptyRegex(),
                    SpanColor = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color) },
                    RuleSet = ruleSet,
                    SpanColorIncludesEnd = true,
                    SpanColorIncludesStart = true
                };

                ruleSet.Spans.Add(span);

                textEditor.TextArea.TextView.Redraw(match.Index, match.Length);
            }
        });
    }

    public static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier())
        {
            return;
        }

        e.Handled = true;

        TextEditor? textEditor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (textEditor == null) return;

        Point pos = e.GetPosition(textEditor.TextArea.TextView) + textEditor.TextArea.TextView.ScrollOffset;
        SimpleSegment word = TextEditorUtilities.GetWordAtMousePosition(pos, textEditor.TextArea);

        if (word != SimpleSegment.Invalid)
        {
            textEditor.Select(word.Offset, word.Length);
        }
    }

    public static void OnTextPasting(object? sender, TextEventArgs e)
    {
        if (!SkEditorAPI.Core.GetAppConfig().IsPasteIndentationEnabled)
        {
            return;
        }

        string properText = e.Text; // TODO: Handle bad indented copied code
        if (!properText.Contains(Environment.NewLine) || properText.Contains('\n') || properText.Contains('\r'))
        {
            e.Text = properText;
            return;
        }

        TextEditor? textEditor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (textEditor == null) return;
        
        DocumentLine line = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);

        string lineText = textEditor.Document.GetText(line);
        string indentation = "";
        foreach (char c in lineText)
        {
            if (char.IsWhiteSpace(c))
            {
                indentation += c;
            }
            else
            {
                break;
            }
        }

        string[] pastes = properText.Split([Environment.NewLine], StringSplitOptions.None);

        if (pastes.Length == 1)
        {
            e.Text = indentation + properText;
            return;
        }

        StringBuilder sb = new();
        foreach (string paste in pastes)
        {
            sb.AppendLine(indentation + paste);
        }

        e.Text = sb.ToString().Trim();
    }

    [GeneratedRegex(@"<(color:|#)?#?(?:[0-9a-fA-F]{3}){1,2}>", RegexOptions.Compiled)]
    private static partial Regex HexRegex();

    [GeneratedRegex("")]
    private static partial Regex EmptyRegex();

    [GeneratedRegex(CommentPattern, RegexOptions.Compiled)]
    private static partial Regex CommentRegex();
}