using System.ComponentModel;
using AvaloniaEdit.Document;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public class SaveIndicatorFeature : IEditorFeature
{
    private PropertyChangedEventHandler? _handler;

    public void Attach(FileTab tab)
    {
        _handler = (_, e) =>
        {
            if (e.PropertyName == nameof(UndoStack.IsOriginalFile))
            {
                tab.IsSaved = tab.Document.UndoStack.IsOriginalFile;
            }
        };

        tab.Document.UndoStack.PropertyChanged += _handler;
    }

    public void Detach(FileTab tab)
    {
        tab.Document.UndoStack.PropertyChanged -= _handler;
    }
}