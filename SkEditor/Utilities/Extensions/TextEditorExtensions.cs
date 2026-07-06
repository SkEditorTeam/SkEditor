using System;
using AvaloniaEdit;
using R3;

namespace SkEditor.Utilities.Extensions;

public static class TextEditorExtensions
{
    extension(TextEditor editor)
    {
        public Observable<EventArgs> TextChangedAsObservable()
        {
            return Observable.FromEventHandler(
                    h => editor.Document.TextChanged += h,
                    h => editor.Document.TextChanged -= h
                )
                .Select(x => x.e);
        }
    }
}