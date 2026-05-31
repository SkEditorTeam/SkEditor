using System.ComponentModel;
using AvaloniaEdit.Document;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features;

public class SaveIndicatorFeature(AppSettings appSettings) : IEditorFeature
{
    private PropertyChangedEventHandler? _handler;
    private IFileTab? _fileTab;
    private ISaveStateTab? _saveStateTab;
    private ITextEditorTab? _textEditorTab;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab and ISaveStateTab and IFileTab;
    }

    public void Attach(IFeatureTab tab)
    {
        if (tab is not (ITextEditorTab textEditorTab and ISaveStateTab saveStateTab and IFileTab fileTab))
        {
            return;
        }

        _textEditorTab = textEditorTab;
        _saveStateTab = saveStateTab;
        _fileTab = fileTab;

        _handler = (_, e) =>
        {
            if (e.PropertyName != nameof(UndoStack.IsOriginalFile))
            {
                return;
            }

            if (appSettings.IsAutoSaveEnabled && _fileTab.Path is not null)
            {
                return;
            }

            _saveStateTab.IsSaved = _textEditorTab.Document.UndoStack.IsOriginalFile;
        };

        _textEditorTab.Document.UndoStack.PropertyChanged += _handler;
    }

    public void Detach(IFeatureTab tab)
    {
        if (_textEditorTab != null)
        {
            _textEditorTab.Document.UndoStack.PropertyChanged -= _handler;
        }

        _handler = null;
        _textEditorTab = null;
        _saveStateTab = null;
        _fileTab = null;
    }
}