using System;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Features.Editing;

public class AutoIndentFeature(AppSettings appSettings) : IEditorFeature
{
    public bool CanAttach(FileTab tab)
    {
        return appSettings.IsAutoIndentEnabled;
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextEntered += OnTextEntered;
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextEntered -= OnTextEntered;
    }

    private void OnTextEntered(object? sender, TextInputEventArgs e)
    {
        if (e.Text != "\n")
        {
            return;
        }

        if (sender is not TextArea area)
        {
            return;
        }

        TextDocument? doc = area.Document;
        int caretLine = area.Caret.Line;

        DocumentLine? prevLine = doc.GetLineByNumber(caretLine - 1);
        ReadOnlySpan<char> prevText = doc.GetTextAsMemory(prevLine.Offset, prevLine.Length).Span;

        bool useTabs = appSettings.UseEditorTabsForIndentation;
        int tabSize = appSettings.EditorTabSize;

        if (!prevText.TrimEnd().EndsWith(':'))
        {
            return;
        }

        string indent = useTabs ? "\t" : new string(' ', tabSize);

        if (indent.Length <= 0)
        {
            return;
        }

        int insertOffset = area.Caret.Offset;
        doc.Insert(insertOffset, indent);
        area.Caret.Offset = insertOffset + indent.Length;
    }
}