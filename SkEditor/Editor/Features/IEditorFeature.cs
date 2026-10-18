using AvaloniaEdit;
using SkEditor.Editor.Tabs.Capabilities;

namespace SkEditor.Editor.Features;

public interface IEditorFeature
{
    bool CanAttach(IFeatureTab tab)
    {
        return true;
    }

    void Attach(IFeatureTab tab)
    {
    }

    void Detach(IFeatureTab tab)
    {
    }

    void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
    }

    void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
    }
}