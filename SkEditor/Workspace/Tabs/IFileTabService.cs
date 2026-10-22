using System.Threading.Tasks;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Tabs;

public interface IFileTabService
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
    ///     Updates the details of a file after it has been renamed or moved.
    /// </summary>
    void UpdateFileDetails(string oldPath, string newName, string newPath);

    /// <summary>
    ///     Notifies the service that the file was deleted, and needs to be updated, e.g. its saved status.
    /// </summary>
    void NotifyFileDelete(string filePath);
}