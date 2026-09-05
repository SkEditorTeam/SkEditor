using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using ObservableCollections;
using R3;
using SkEditor.Models.Completions;
using SkEditor.Utilities.Editor.Completions;
using SkEditor.ViewModels.Controls.Editor;

namespace SkEditor.Views.Controls.Editor;

public class SkEditorCompletionWindow : CompletionWindowBase
{
    private readonly IEnumerable<ICompletionProvider> _completionProviders;
    private readonly TextArea _textArea;

    private int _lastOffset = -1;
    public ObservableList<CompletionData> Completions { get; } = [];
    public BindableReactiveProperty<CompletionData?> SelectedCompletion { get; } = new();

    public SkEditorCompletionWindow(TextArea textArea, IEnumerable<ICompletionProvider> completionProviders) :
        base(textArea)
    {
        _textArea = textArea;
        _completionProviders = completionProviders;

        int wordStart = GetWordStartOffset();
        StartOffset = wordStart;

        UpdateCompletionsList();

        CompletionViewModel vm = new(Completions)
        {
            SelectedCompletion = SelectedCompletion
        };

        CompletionView myView = new()
        {
            DataContext = vm
        };
        Child = myView;

        TextArea.Caret.PositionChanged += OnCaretPositionChanged;

        myView.DoubleTapped += (_, e) =>
        {
            if (e.Source is not Visual visual || visual.FindAncestorOfType<ListBoxItem>() is null)
            {
                return;
            }

            AcceptComplete();
            e.Handled = true;
        };
    }

    public new void Show()
    {
        if (Completions.Count == 0)
        {
            Hide();
            return;
        }

        base.Show();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Down:
                SelectNext();
                e.Handled = true;
                break;
            case Key.Up:
                SelectPrevious();
                e.Handled = true;
                break;
            case Key.Enter:
            case Key.Tab:
                AcceptComplete();
                e.Handled = true;
                break;
        }
    }

    protected override void OnClosed()
    {
        base.OnClosed();
        TextArea.Caret.PositionChanged -= OnCaretPositionChanged;
    }

    private int GetWordStartOffset()
    {
        int offset = _textArea.Caret.Offset;
        TextDocument? document = _textArea.Document;
        if (document == null || offset == 0)
        {
            return offset;
        }

        int wordStart = TextUtilities.GetNextCaretPosition(
            document,
            offset,
            LogicalDirection.Backward,
            CaretPositioningMode.WordStart);

        return wordStart < 0 ? offset : wordStart;
    }

    private void UpdateCompletionsList()
    {
        Completions.Clear();

        int globalOffset = _textArea.Caret.Offset;
        TextDocument? document = _textArea.Document;
        if (document == null)
        {
            return;
        }

        DocumentLine? line = document.GetLineByOffset(globalOffset);
        int lineStartOffset = StartOffset - line.Offset;
        int length = Math.Max(0, globalOffset - StartOffset);
        ReadOnlySpan<char> word = document.GetTextAsMemory(StartOffset, length).Span;

        if (word.IsWhiteSpace() || word.IndexOfAny(' ', '\t') >= 0)
        {
            return;
        }

        CompletionContext context = new(StartOffset, lineStartOffset, length, word);

        List<CompletionData> completions = [];
        foreach (ICompletionProvider completionProvider in _completionProviders)
        {
            if (!completionProvider.CheckCompletionsAvailable(document, context))
            {
                continue;
            }

            completionProvider.PopulateCompletions(document, context, completions);
        }

        Completions.AddRange(completions);

        SelectedCompletion.Value = Completions.FirstOrDefault();
    }

    private void OnCaretPositionChanged(object? sender, EventArgs e)
    {
        int offset = TextArea.Caret.Offset;

        if (offset == _lastOffset)
        {
            return;
        }

        _lastOffset = offset;

        if (offset < StartOffset || offset > EndOffset)
        {
            Hide();
            return;
        }

        UpdateCompletionsList();

        if (Completions.Count == 0)
        {
            Hide();
        }
    }

    private void AcceptComplete()
    {
        if (SelectedCompletion.Value is null)
        {
            return;
        }

        int offset = _textArea.Caret.Offset;
        int lengthToReplace = offset - StartOffset;

        _textArea.Document.Replace(StartOffset, lengthToReplace, SelectedCompletion.Value.Text);

        Hide();
    }

    private void SelectNext()
    {
        if (SelectedCompletion.Value is null || Completions.Count == 0)
        {
            return;
        }

        int index = Completions.IndexOf(SelectedCompletion.Value);
        SelectedCompletion.Value = index + 1 < Completions.Count
            ? Completions[index + 1]
            : Completions[0];
    }

    private void SelectPrevious()
    {
        if (SelectedCompletion.Value is null || Completions.Count == 0)
        {
            return;
        }

        int index = Completions.IndexOf(SelectedCompletion.Value);
        SelectedCompletion.Value = index - 1 >= 0
            ? Completions[index - 1]
            : Completions[^1];
    }
}