using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features;

public class EditorRedrawFeature : IEditorFeature
{
    private TextEditor? _editor;
    private ITextEditorTab? _tab;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not ITextEditorTab textEditorTab)
        {
            return;
        }

        _tab = textEditorTab;
        _editor = editor;

        _tab.RedrawRequested += OnRedrawRequested;
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        if (_tab != null)
        {
            _tab.RedrawRequested -= OnRedrawRequested;
        }

        _tab = null;
        _editor = null;
    }

    private void OnRedrawRequested(ISegment? segment)
    {
        if (segment is not null)
        {
            _editor?.TextArea.TextView.Redraw(segment);
        }
        else
        {
            _editor?.TextArea.TextView.Redraw();
        }
    }
}