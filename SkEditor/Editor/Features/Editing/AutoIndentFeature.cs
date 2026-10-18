using System;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using SkEditor.Configuration.Configs;
using SkEditor.Editor.Tabs.Capabilities;

namespace SkEditor.Editor.Features.Editing;

public class AutoIndentFeature(AppSettings appSettings) : IEditorFeature
{
    private bool _undoGroupOpen;

    public bool CanAttach(IFeatureTab tab)
    {
        return appSettings.IsAutoIndentEnabled && tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        editor.TextArea.TextEntering += OnTextEntering;
        editor.TextArea.TextEntered += OnTextEntered;
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        editor.TextArea.TextEntering -= OnTextEntering;
        editor.TextArea.TextEntered -= OnTextEntered;
    }

    private void OnTextEntering(object? sender, TextInputEventArgs e)
    {
        if (e.Text != "\n")
        {
            return;
        }

        if (sender is not TextArea area)
        {
            return;
        }

        area.Document.UndoStack.StartUndoGroup();
        _undoGroupOpen = true;
    }

    private void OnTextEntered(object? sender, TextInputEventArgs e)
    {
        if (!_undoGroupOpen)
        {
            return;
        }

        if (sender is not TextArea area)
        {
            return;
        }

        try
        {
            if (e.Text != "\n")
            {
                return;
            }

            TextDocument doc = area.Document;
            int caretLine = area.Caret.Line;

            DocumentLine prevLine = doc.GetLineByNumber(caretLine - 1);
            ReadOnlySpan<char> prevText =
                doc.GetTextAsMemory(prevLine.Offset, prevLine.Length).Span;

            if (!prevText.TrimEnd().EndsWith(':'))
            {
                return;
            }

            bool useTabs = appSettings.UseEditorTabsForIndentation;
            int tabSize = appSettings.EditorTabSize;
            string indent = useTabs ? "\t" : new string(' ', tabSize);

            if (indent.Length == 0)
            {
                return;
            }

            int insertOffset = area.Caret.Offset;
            doc.Insert(insertOffset, indent);
            area.Caret.Offset = insertOffset + indent.Length;
        }
        finally
        {
            area.Document.UndoStack.EndUndoGroup();
            _undoGroupOpen = false;
        }
    }
}