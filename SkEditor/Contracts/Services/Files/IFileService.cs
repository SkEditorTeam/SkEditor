using System.Threading.Tasks;

namespace SkEditor.Contracts.Services.Files;

public interface IFileService
{
    /// <summary>
    ///     Creates a new tab with an empty file.
    /// </summary>
    void NewFile();

    /// <summary>
    ///     Opens a file dialog to select a file and opens it in a new tab.
    /// </summary>
    Task OpenFileAsync();

    /// <summary>
    ///     Opens the file at the specified path in a new tab.
    /// </summary>
    /// <param name="path">The path of the file to open.</param>
    Task OpenFileAsync(string path);

    /// <summary>
    ///     Saves the currently active file. If the file has not been saved before, it prompts the user to select a location to
    ///     save the file.
    /// </summary>
    Task SaveFileAsync();

    /// <summary>
    ///     Prompts the user to select a location and saves the file at the specified location, regardless of whether it has
    ///     been saved before or not.
    /// </summary>
    Task SaveAsFileAsync();
}