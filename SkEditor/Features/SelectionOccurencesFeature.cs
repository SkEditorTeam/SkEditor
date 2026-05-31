using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.Contracts.Services;
using SkEditor.Tranformers;
using SkEditor.Utilities.Editor;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Features;

public class SelectionOccurencesFeature(IThemeService themes) : IEditorFeature
{
    private TextEditor? _editor;

    public void OnViewAttached(FileTabViewModel tab, TextEditor editor)
    {
        _editor = editor;
        editor.TextArea.SelectionChanged += OnSelectionChanged;
    }

    public void OnViewDetached(FileTabViewModel tab, TextEditor editor)
    {
        _editor = null;
        editor.TextArea.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (_editor == null)
        {
            return;
        }

        _editor.TextArea.TextView.LineTransformers
            .Where(x => x is OccurenceBackgroundTransformer)
            .ToList()
            .ForEach(x => _editor.TextArea.TextView.LineTransformers.Remove(x));

        int start = _editor.SelectionStart;
        int length = _editor.SelectionLength;
        string selectedText = _editor.Document.GetText(start, length);

        if (string.IsNullOrWhiteSpace(selectedText))
        {
            return;
        }

        IEnumerable<SimpleSegment> wordOccurrences =
            TextEditorUtilities.GetWordOccurrences(selectedText, _editor.Document);
        IList<IVisualLineTransformer>? lineTransformers = _editor.TextArea.TextView.LineTransformers;

        foreach (SimpleSegment segment in wordOccurrences.Where(s => s.Offset != start || s.Length != length))
        {
            int lineNumber = _editor.Document.GetLineByOffset(segment.Offset).LineNumber;
            lineTransformers.Add(new OccurenceBackgroundTransformer(themes)
            {
                LineNumber = lineNumber,
                StartOffset = segment.Offset,
                EndOffset = segment.Offset + segment.Length
            });
        }
    }
}