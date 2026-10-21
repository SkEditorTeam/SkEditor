using System.Threading.Tasks;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Files;

public interface IFileService
{
    /// <summary>
    ///     Creates a new tab with an empty file.
    /// </summary>
    ITab NewFile();

    /// <summary>
    ///     Opens a file dialog to select a file and opens it in a new tab.
    /// </summary>
    Task OpenFileAsync();

    /// <summary>
    ///     Opens the specified file in a new tab.
    /// </summary>
    Task OpenFileAsync(BaseFile file);

    /// <summary>
    ///     Loads and opens the specified pre-constructed file tab.
    /// </summary>
    /// <param name="fileTab">The tab to open.</param>
    /// <param name="select">Whether to select the tab after opening.</param>
    Task LoadAndOpenFileTabAsync(IFileTab fileTab, bool select = true);

    /// <summary>
    ///     Tries to load the file content.
    /// </summary>
    /// <returns>True if success</returns>
    Task<bool> LoadAndReadFileAsync(IFileTab fileTab);

    /// <summary>
    ///     Updates the details of a file after it has been renamed or moved.
    /// </summary>
    void UpdateFileDetails(string oldPath, string newName, string newPath);

    /// <summary>
    ///     Notifies the service that the file was deleted, and needs to be updated, e.g. its saved status.
    /// </summary>
    void NotifyFileDelete(string filePath);

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