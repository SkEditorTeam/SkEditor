using System.Threading.Tasks;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Files;

public interface IFileService
{
    Task<bool> LoadAndReadFileAsync(IFileTab fileTab);

    /// <summary>
    ///     Saves the file. If the file has not been saved before, it prompts the user to select a location to save the file.
    /// </summary>
    Task<bool> SaveFileAsync(IFileTab fileTab);

    /// <summary>
    ///     Prompts the user to select a location and saves the file at the specified location, regardless of whether it has
    ///     been saved before or not.
    /// </summary>
    Task<bool> SaveFileAsAsync(IFileTab fileTab);
}