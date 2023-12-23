using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Editor;
public partial class TextEditorEventHandler
{
    private static readonly Dictionary<string, string> _symbolPairs = new()
    {
        { "(", ")" },
        { "[", "]" },
        { "\"", "\"" },
        { "<", ">" },
        { "{", "}" },
    };

    private const string commentPattern = @"#(?!#(?:\s*#[^#]*)?)\s*[^#]*$";
    private static Regex _commentRegex = new(commentPattern, RegexOptions.Compiled);

    public static Dictionary<TextEditor, ScrollViewer> ScrollViewers { get; } = [];

    public static void OnZoom(object sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier()) return;

        e.Handled = true;

        TextEditor editor = ApiVault.Get().GetTextEditor();

        int zoom = (e.Delta.Y > 0 && editor.FontSize < 200) ? 1 : -1;

        Zoom(editor, zoom);
    }

    public static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier()) return;

        TextEditor editor = ApiVault.Get().GetTextEditor();

        if (e.Key == Key.OemPlus) Zoom(editor, 5);
        else if (e.Key == Key.OemMinus) Zoom(editor, -5);
    }

    private static void Zoom(TextEditor editor, int value)
    {
        if (value < 0 && editor.FontSize <= 5) return;

        ScrollViewer scrollViewer = GetScrollViewer(editor);
        double oldLineHeight = editor.TextArea.TextView.DefaultLineHeight;
        editor.FontSize += value;
        double lineHeight = editor.TextArea.TextView.DefaultLineHeight;

        double lineHeightChange = lineHeight / oldLineHeight;
        double newOffset = scrollViewer.Offset.Y * lineHeightChange;

        scrollViewer.SetCurrentValue(ScrollViewer.OffsetProperty, new Vector(scrollViewer.Offset.X, newOffset));
    }

    private static ScrollViewer GetScrollViewer(TextEditor editor)
    {
        if (ScrollViewers.TryGetValue(editor, out ScrollViewer? value)) return value;

        Type type = editor.GetType();
        PropertyInfo propInfo = type.GetProperty("ScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic);
        if (propInfo == null) return null;

        ScrollViewer scrollViewer = (ScrollViewer)propInfo.GetValue(editor);
        ScrollViewers[editor] = scrollViewer;
        return scrollViewer;
    }

    public async static void OnTextChanged(object sender, EventArgs e)
    {
        TabViewItem tab = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;

        if (ApiVault.Get().GetAppConfig().IsAutoSaveEnabled && !string.IsNullOrEmpty(tab.Tag.ToString()))
        {
            await Dispatcher.UIThread.InvokeAsync(FileHandler.SaveFile);
            return;
        }

        if (tab.Header.ToString().EndsWith('*')) return;

        tab.Header += "*";
    }

    public static void DoAutoIndent(object? sender, TextInputEventArgs e)
    {
        if (!ApiVault.Get().GetAppConfig().IsAutoIndentEnabled) return;
        if (!string.IsNullOrWhiteSpace(e.Text)) return;

        TextEditor textEditor = ApiVault.Get().GetTextEditor();

        DocumentLine line = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
        if (!string.IsNullOrWhiteSpace(textEditor.Document.GetText(line))) return;
        if (line.PreviousLine == null) return;

        DocumentLine previousLine = line.PreviousLine;

        string previousLineText = textEditor.Document.GetText(previousLine);
        previousLineText = _commentRegex.Replace(previousLineText, "");
        previousLineText = previousLineText.TrimEnd();

        if (!previousLineText.EndsWith(':')) return;

        textEditor.Document.Insert(line.Offset, "\t");
    }

    public static void DoAutoPairing(object? sender, TextInputEventArgs e)
    {
        if (!ApiVault.Get().GetAppConfig().IsAutoPairingEnabled) return;

        string symbol = e.Text;
        if (!_symbolPairs.TryGetValue(symbol, out string? value)) return;

        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        if (textEditor.Document.TextLength > textEditor.CaretOffset)
        {
            string nextChar = textEditor.Document.GetText(textEditor.CaretOffset, 1);
            if (nextChar.Equals(value)) return;
        }

        textEditor.Document.Insert(textEditor.CaretOffset, value);
        textEditor.CaretOffset--;
    }

    public static async void CheckForHex(object? sender, EventArgs e)
    {
        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        TextDocument document = textEditor.Document;

        Regex regex = HexRegex();

        Dispatcher.UIThread.Post(() =>
        {
            MatchCollection matches = regex.Matches(textEditor.Text);

            foreach (Match match in matches.Cast<Match>())
            {
                string hex = match.Value[2..^1];
                bool parsed = Color.TryParse(hex, out Color color);
                if (!parsed) continue;

                HighlightingRuleSet ruleSet = textEditor.SyntaxHighlighting.GetNamedRuleSet("BracedExpressionAndColorsRuleSet");
                if (ruleSet == null) continue;

                if (ruleSet.Rules.Any(r => r is HighlightingRule rule && rule.Regex.ToString().Contains(hex))) continue;

                HighlightingSpan span = new()
                {
                    StartExpression = new Regex(@"<#" + hex + @">"),
                    EndExpression = EmptyRegex(),
                    SpanColor = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color) },
                    RuleSet = ruleSet,
                    SpanColorIncludesEnd = true,
                    SpanColorIncludesStart = true,
                };

                ruleSet.Spans.Add(span);

                textEditor.TextArea.TextView.Redraw(match.Index, match.Length);
            }
        });
    }

    public static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier()) return;
        e.Handled = true;

        TextEditor textEditor = ApiVault.Get().GetTextEditor();

        Point pos = e.GetPosition(textEditor.TextArea.TextView) + textEditor.TextArea.TextView.ScrollOffset;
        SimpleSegment word = TextEditorUtilities.GetWordAtMousePosition(pos, textEditor.TextArea);

        if (word != SimpleSegment.Invalid)
        {
            textEditor.Select(word.Offset, word.Length);
        }
    }

    [GeneratedRegex(@"<##(?:[0-9a-fA-F]{3}){1,2}>", RegexOptions.Compiled)]
    private static partial Regex HexRegex();
    [GeneratedRegex("")]
    private static partial Regex EmptyRegex();
}