using SkEditor.Models.Tabs;

namespace SkEditor.Features;

public interface IEditorFeature
{
    bool CanAttach(FileTab tab);

    void Attach(FileTab tab);
    void Detach(FileTab tab);
}