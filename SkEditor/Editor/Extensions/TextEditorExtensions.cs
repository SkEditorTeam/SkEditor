using System;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using R3;

namespace SkEditor.Editor.Extensions;

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

        public Observable<TextInputEventArgs> TextEnteredAsObservable()
        {
            return Observable.FromEventHandler<TextInputEventArgs>(
                    h => editor.TextArea.TextEntered += h,
                    h => editor.TextArea.TextEntered -= h
                )
                .Select(x => x.e);
        }

        public Observable<TextInputEventArgs> TextEnteringAsObservable()
        {
            return Observable.FromEventHandler<TextInputEventArgs>(
                    h => editor.TextArea.TextEntering += h,
                    h => editor.TextArea.TextEntering -= h
                )
                .Select(x => x.e);
        }

        public Observable<KeyEventArgs> KeyDownAsObservable()
        {
            return Observable.FromEventHandler<KeyEventArgs>(
                    h => editor.TextArea.KeyDown += h,
                    h => editor.TextArea.KeyDown -= h
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

        public Observable<PointerEventArgs> PointerExitedAsObservable()
        {
            return Observable.FromEventHandler<PointerEventArgs>(
                    h => editor.TextArea.TextView.PointerExited += h,
                    h => editor.TextArea.TextView.PointerExited -= h
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