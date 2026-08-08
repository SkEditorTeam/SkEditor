using System;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
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

        public Observable<PointerEventArgs> PointerMovedAsObservable()
        {
            return Observable.FromEventHandler<PointerEventArgs>(
                    h => editor.TextArea.TextView.PointerMoved += h,
                    h => editor.TextArea.TextView.PointerMoved -= h
                )
                .Select(x => x.e);
        }

        /// <summary>
        ///     Gets the length of the non-whitespace content of the specified line
        ///     and the column with its first non-whitespace character.
        /// </summary>
        public (int length, int notEmptyColumn) GetLineLengthAndNotEmptyColumn(DocumentLine line)
        {
            int notEmptyColumn = 0;
            int length = line.Length;
            for (int i = 0; i < line.Length; i++)
            {
                if (char.IsWhiteSpace(editor.Document.GetCharAt(line.Offset + i)))
                {
                    continue;
                }

                notEmptyColumn = i + 1;
                length -= i;
                break;
            }

            for (int i = line.Length - 1; i >= 0; i--)
            {
                if (char.IsWhiteSpace(editor.Document.GetCharAt(line.Offset + i)))
                {
                    continue;
                }

                length -= line.Length - i - 1;
                break;
            }

            return (length, notEmptyColumn);
        }
    }
}