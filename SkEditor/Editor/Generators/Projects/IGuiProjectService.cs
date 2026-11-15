using System.Threading.Tasks;
using ObservableCollections;
using R3;

namespace SkEditor.Editor.Generators.Projects;

public interface IGuiProjectService
{
    ReadOnlyReactiveProperty<GuiProject?> ActiveProject { get; }
    ObservableList<GuiProject> Projects { get; }
    
    Task LoadProjects();
    Task SaveProject(GuiProject project);
    void SetActiveProject(GuiProject? project);
    void DeleteProject(GuiProject project);
    Task RenameProject(GuiProject project, string name);
}