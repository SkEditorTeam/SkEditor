using System.Threading.Tasks;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Files;

public interface IFileService
{
    Task<bool> LoadAndReadFileAsync(IFileTab fileTab);

    /// <summary>
    ///     Performs the actual file save operation
    /// </summary>
    /// <returns>True if success, false otherwise (failed)</returns>
    Task<bool> PerformSaveAsync(IFileTab fileTab);
}