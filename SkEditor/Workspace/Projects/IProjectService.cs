using System.Threading.Tasks;
using SkEditor.Models.Projects.Local;

namespace SkEditor.Contracts.Services.Files;

public interface IProjectService
{
    LocalFileSystem? CurrentProject { get; }
    string? CurrentProjectPath { get; }
    
    Task<bool> OpenProjectAsync();
    void OpenProject(string path);
    void CloseProject();
}