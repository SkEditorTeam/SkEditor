using System.Threading.Tasks;
using SkEditor.Workspace.Filesystem.Local;

namespace SkEditor.Workspace.Projects;

public interface IProjectService
{
    LocalNodeFileSystem? CurrentProject { get; }
    string? CurrentProjectPath { get; }
    
    Task<bool> OpenProjectAsync();
    void OpenProject(string path);
    void CloseProject();
}