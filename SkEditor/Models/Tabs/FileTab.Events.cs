using System;
using AvaloniaEdit.Document;
using SkEditor.Utilities.Editor;

namespace SkEditor.Models.Tabs;

public partial class FileTab
{
    public event Action<EditorCommandType>? EditorCommandRequested;

    public void RequestEditorCommand(EditorCommandType commandType)
    {
        EditorCommandRequested?.Invoke(commandType);
    }

    public event Action<ISegment?>? RedrawRequested;

    public void RequestRedraw(ISegment? segment = null)
    {
        RedrawRequested?.Invoke(segment);
    }

    public event Action<TextLocation>? GoToLocationRequested;

    public void RequestGoToLocation(TextLocation location)
    {
        GoToLocationRequested?.Invoke(location);
    }

    public event Action? FocusRequested;

    public void RequestFocus()
    {
        FocusRequested?.Invoke();
    }
}