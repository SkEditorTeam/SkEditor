using AvaloniaEdit;
using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public interface IEditorFeature
{
    bool CanAttach(FileTab tab)
    {
        return true;
    }

    void Attach(FileTab tab) {}
    void Detach(FileTab tab) {}

    void OnViewAttached(FileTab tab, TextEditor editor)
    {
    }

    void OnViewDetached(FileTab tab, TextEditor editor)
    {
    }
}