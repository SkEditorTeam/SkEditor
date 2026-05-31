using AvaloniaEdit;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Features;

public interface IEditorFeature
{
    bool CanAttach(FileTabViewModel tab)
    {
        return true;
    }

    void Attach(FileTabViewModel tab)
    {
    }

    void Detach(FileTabViewModel tab)
    {
    }

    void OnViewAttached(FileTabViewModel tab, TextEditor editor)
    {
    }

    void OnViewDetached(FileTabViewModel tab, TextEditor editor)
    {
    }
}