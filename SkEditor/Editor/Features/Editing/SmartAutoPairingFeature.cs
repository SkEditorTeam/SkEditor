using System.Collections.Generic;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features.Editing;

public class SmartAutoPairingFeature(AppSettings appSettings) : IEditorFeature
{
    internal static readonly Dictionary<char, char> SymbolPairs = new()
    {
        { '(', ')' },
        { '[', ']' },
        { '"', '"' },
        { '<', '>' },
        { '{', '}' },
        { '%', '%' }
    };

    private BracketEditingHandler? _bracketHandler;

    public bool CanAttach(IFeatureTab tab)
    {
        return appSettings.IsAutoPairingEnabled && tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        editor.TextArea.TextEntering += OnTextEntering;

        _bracketHandler = new BracketEditingHandler(editor.TextArea);
        editor.TextArea.PushStackedInputHandler(_bracketHandler);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        editor.TextArea.TextEntering -= OnTextEntering;

        if (_bracketHandler == null)
        {
            return;
        }

        editor.TextArea.PopStackedInputHandler(_bracketHandler);
        _bracketHandler = null;
    }

    private static void OnTextEntering(object? sender, TextInputEventArgs e)
    {
        char? symbol = e.Text?[0];
        if (symbol == null || sender is not TextArea area)
        {
            return;
        }

        int caretOffset = area.Caret.Offset;
        if (HandleStepOver(e, caretOffset, area, symbol.Value))
        {
            return;
        }

        if (!SymbolPairs.TryGetValue(symbol.Value, out char closingSymbol))
        {
            return;
        }

        if (HandleSelectionWrap(e, area, symbol.Value, closingSymbol))
        {
            return;
        }

        if (NextToLetterOrDigit(caretOffset, area))
        {
            return;
        }

        area.Document.Insert(caretOffset, closingSymbol.ToString());
        area.Caret.Offset = caretOffset;
    }

    private static bool HandleStepOver(TextInputEventArgs e, int caretOffset, TextArea area, char symbol)
    {
        if (caretOffset >= area.Document.TextLength)
        {
            return false;
        }

        char nextChar = area.Document.GetCharAt(caretOffset);
        if (symbol != nextChar || !SymbolPairs.ContainsValue(symbol))
        {
            return false;
        }

        area.Caret.Offset++;
        e.Handled = true;
        return true;
    }

    private static bool HandleSelectionWrap(TextInputEventArgs e, TextArea area, char symbol, char closingSymbol)
    {
        if (area.Selection.IsEmpty)
        {
            return false;
        }

        string selectedText = area.Selection.GetText();
        int startOffset = area.Selection.SurroundingSegment.Offset;
        int textLength = selectedText.Length;

        area.Selection.ReplaceSelectionWithText($"{symbol}{selectedText}{closingSymbol}");

        area.Caret.Offset = startOffset + 1 + textLength;
        area.Selection = Selection.Create(area, startOffset + 1, startOffset + 1 + textLength);

        e.Handled = true;
        return true;
    }

    private static bool NextToLetterOrDigit(int caretOffset, TextArea area)
    {
        if (caretOffset >= area.Document.TextLength)
        {
            return false;
        }

        char nextChar = area.Document.GetCharAt(caretOffset);
        return char.IsLetterOrDigit(nextChar);
    }
}

public class BracketEditingHandler(TextArea textArea) : TextAreaStackedInputHandler(textArea)
{
    public override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key != Key.Back)
        {
            return;
        }

        TextDocument? doc = TextArea.Document;
        int offset = TextArea.Caret.Offset;

        if (offset < 1)
        {
            return;
        }

        char before = doc.GetCharAt(offset - 1);
        char after = offset < doc.TextLength ? doc.GetCharAt(offset) : '\0';

        if (!IsPair(before, after))
        {
            return;
        }

        e.Handled = true;
        doc.Remove(offset - 1, 2);
        TextArea.Caret.Offset = offset - 1;
    }

    private static bool IsPair(char open, char close)
    {
        return SmartAutoPairingFeature.SymbolPairs.TryGetValue(open, out char expectedClose) && expectedClose == close;
    }
}