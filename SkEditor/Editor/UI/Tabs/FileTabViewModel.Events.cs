using System;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Editor.Commands;

namespace SkEditor.Editor.UI.Tabs;

public partial class FileTabViewModel
{
    [RelayCommand]
    public void RequestEditorCommand(EditorCommandType commandType)
    {
        EditorCommandRequested?.Invoke(commandType);
    }

    public void RequestRedraw(ISegment? segment = null)
    {
        RedrawRequested?.Invoke(segment);
    }

    public void RequestGoToLocation(TextLocation location)
    {
        GoToLocationRequested?.Invoke(location);
    }

    public void RequestFocus()
    {
        FocusRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public void InsertCode(string code)
    {
        int caretOffset = CaretOffset;
        TextLocation location = Document.GetLocation(caretOffset);
        DocumentLine? line = Document.GetLineByNumber(location.Line);
        if (line is null)
        {
            return;
        }

        if (line.Length > 0)
        {
            Document.Insert(line.EndOffset, "\n" + code);
        }
        else
        {
            Document.Insert(caretOffset, code);
        }
    }

    public event Action<EditorCommandType>? EditorCommandRequested;

    public event Action<ISegment?>? RedrawRequested;

    public event Action<TextLocation>? GoToLocationRequested;

    public event EventHandler? FocusRequested;
}