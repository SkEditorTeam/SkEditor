using R3;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Editor.Features.Saving;

public class SaveIndicatorFeature : IEditorFeature
{
    private ISaveStateTab? _saveStateTab;
    private ITextEditorTab? _textEditorTab;
    private DisposableBag _disposables;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab and ISaveStateTab and IFileTab;
    }

    public void Attach(IFeatureTab tab)
    {
        if (tab is not (ITextEditorTab textEditorTab and ISaveStateTab saveStateTab and IFileTab))
        {
            return;
        }

        _textEditorTab = textEditorTab;
        _saveStateTab = saveStateTab;
        _disposables = default;

        _textEditorTab.Document.UndoStack
            .ObservePropertyChanged(x => x.IsOriginalFile)
            .Subscribe(_ =>
            {
                _saveStateTab.IsSaved = _textEditorTab.Document.UndoStack.IsOriginalFile;
            })
            .AddTo(ref _disposables);
    }

    public void Detach(IFeatureTab tab)
    {
        _disposables.Dispose();
        _textEditorTab = null;
        _saveStateTab = null;
    }
}