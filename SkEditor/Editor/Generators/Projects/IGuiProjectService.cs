using System.Threading.Tasks;
using ObservableCollections;

namespace SkEditor.Editor.Generators.Projects;

public interface IGuiProjectService
{
    GuiProject? ActiveProject { get; }
    ObservableList<GuiProject> Projects { get; }
    
    Task LoadProjects();
    Task SaveProject(GuiProject project);
    void SetActiveProject(GuiProject project);
}