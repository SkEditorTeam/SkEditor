using System.ComponentModel;
using AvaloniaEdit.Document;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Features;

public class SaveIndicatorFeature(AppSettings appSettings) : IEditorFeature
{
    private PropertyChangedEventHandler? _handler;

    public void Attach(IFeatureTab tab)
    {
        _handler = (_, e) =>
        {
            if (e.PropertyName != nameof(UndoStack.IsOriginalFile))
            {
                return;
            }

            if (appSettings.IsAutoSaveEnabled && tab.File.Path is not null)
            {
                return;
            }

            tab.IsSaved = tab.Document.UndoStack.IsOriginalFile;
        };

        tab.Document.UndoStack.PropertyChanged += _handler;
    }

    public void Detach(IFeatureTab tab)
    {
        tab.Document.UndoStack.PropertyChanged -= _handler;
    }
}