using System.Threading.Tasks;
using ObservableCollections;
using R3;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.Services;

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