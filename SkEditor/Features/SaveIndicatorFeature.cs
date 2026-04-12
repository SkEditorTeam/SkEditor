using System;
using System.Collections.Generic;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public class SaveIndicatorFeature : IEditorFeature
{
    private readonly Dictionary<FileTab, EventHandler> _handlers = [];

    public bool CanAttach(FileTab tab) => true;

    public void Attach(FileTab tab)
    {
        EventHandler handler = (_, _) => { tab.IsSaved = false; };

        _handlers[tab] = handler;

        tab.Document.TextChanged += handler;
    }

    public void Detach(FileTab tab)
    {
        if (!_handlers.TryGetValue(tab, out var handler))
        {
            return;
        }

        tab.Document.TextChanged -= handler;
        _handlers.Remove(tab);
    }
}