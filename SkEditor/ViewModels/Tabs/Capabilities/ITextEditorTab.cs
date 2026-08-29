using System;
using Avalonia;
using AvaloniaEdit.Document;
using R3;
using SkEditor.Utilities.Editor;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface ITextEditorTab
{
    int CaretOffset { get; set; }
    TextDocument Document { get; }
    double FontSize { get; set; }
    Vector ScrollOffset { get; set; }
    BindableReactiveProperty<bool> WordWrap { get; }

    event Action<EditorCommandType>? EditorCommandRequested;
    event Action<ISegment?>? RedrawRequested;
    event Action<TextLocation>? GoToLocationRequested;
}