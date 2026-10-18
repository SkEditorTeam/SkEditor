using System;
using AvaloniaEdit.Document;
using SkEditor.Utilities.Editor;

namespace SkEditor.ViewModels.Tabs;

public partial class FileTabViewModel
{
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

    public event Action<EditorCommandType>? EditorCommandRequested;

    public event Action<ISegment?>? RedrawRequested;

    public event Action<TextLocation>? GoToLocationRequested;

    public event EventHandler? FocusRequested;
}