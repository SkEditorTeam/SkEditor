using System.Collections.Generic;
using System.ComponentModel;
using AvaloniaEdit.Document;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public class SaveIndicatorFeature : IEditorFeature
{
    private readonly Dictionary<FileTab, PropertyChangedEventHandler> _handlers = [];

    public bool CanAttach(FileTab tab) => true;

    public void Attach(FileTab tab)
    {
        PropertyChangedEventHandler handler = (_, e) =>
        {
            if (e.PropertyName == nameof(UndoStack.IsOriginalFile))
            {
                tab.IsSaved = tab.Document.UndoStack.IsOriginalFile;
            }
        };

        _handlers[tab] = handler;

        tab.Document.UndoStack.PropertyChanged += handler;
    }

    public void Detach(FileTab tab)
    {
        if (!_handlers.TryGetValue(tab, out var handler))
        {
            return;
        }

        tab.Document.UndoStack.PropertyChanged -= handler;
        _handlers.Remove(tab);
    }
}