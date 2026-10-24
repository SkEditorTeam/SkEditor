using System;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.Core.Extensions;

namespace SkEditor.Editor.Commands;

public class TransformCommand : IEditorCommandHandler
{
    private static readonly char[] SmallCapsMap =
    [
        'ᴀ',
        'ʙ',
        'ᴄ',
        'ᴅ',
        'ᴇ',
        'ꜰ',
        'ɢ',
        'ʜ',
        'ɪ',
        'ᴊ',
        'ᴋ',
        'ʟ',
        'ᴍ',
        'ɴ',
        'ᴏ',
        'ᴘ',
        'ꞯ',
        'ʀ',
        'ꜱ',
        'ᴛ',
        'ᴜ',
        'ᴠ',
        'ᴡ',
        'x',
        'ʏ',
        'ᴢ'
    ];

    public EditorCommandType[] HandledCommandTypes =>
    [
        EditorCommandType.TransformToUppercase, EditorCommandType.TransformToLowercase,
        EditorCommandType.TransformToTitlecase, EditorCommandType.TransformToSmallCaps
    ];

    public void Execute(TextDocument document, TextEditor editor, EditorCommandType commandType)
    {
        int selectionStart = editor.SelectionStart;
        int selectionLength = editor.SelectionLength;
        
        if (selectionLength == 0) return;

        ReadOnlySpan<char> selectedText = document.GetTextAsMemory(selectionStart, selectionLength).Span;
        Span<char> transformed = selectionLength <= 512 
            ? stackalloc char[selectionLength] 
            : new char[selectionLength];

        switch (commandType)
        {
            case EditorCommandType.TransformToUppercase:
            {
                selectedText.ToUpperInvariant(transformed);
                break;
            }
            case EditorCommandType.TransformToLowercase:
            {
                selectedText.ToLowerInvariant(transformed);
                break;
            }
            case EditorCommandType.TransformToTitlecase:
            {
                selectedText.ToTitleCase(transformed);
                break;
            }
            case EditorCommandType.TransformToSmallCaps:
            {
                ToSmallCaps(selectedText, transformed);
                break;
            }
        }

        document.Replace(selectionStart, selectionLength, transformed.ToString());
        editor.Select(selectionStart, transformed.Length);
    }

    public static void ToSmallCaps(ReadOnlySpan<char> source, Span<char> destination)
    {
        if (source.Length > destination.Length)
        {
            throw new ArgumentException("Destination span is too short.");
        }

        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            char lower = char.ToLowerInvariant(c);

            if (lower is >= 'a' and <= 'z')
            {
                destination[i] = SmallCapsMap[lower - 'a'];
            }
            else
            {
                destination[i] = c;
            }
        }
    }
}