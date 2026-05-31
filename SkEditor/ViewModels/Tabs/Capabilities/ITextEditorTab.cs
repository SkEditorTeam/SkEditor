using System;
using Avalonia;
using AvaloniaEdit.Document;
using SkEditor.Utilities.Editor;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface ITextEditorTab
{
    int CaretOffset { get; set; }
    Vector ScrollOffset { get; set; }
    bool WordWrap { get; set; }
    TextDocument Document { get; }

    event Action<EditorCommandType>? EditorCommandRequested;
    event Action<ISegment?>? RedrawRequested;
    event Action<TextLocation>? GoToLocationRequested;
}