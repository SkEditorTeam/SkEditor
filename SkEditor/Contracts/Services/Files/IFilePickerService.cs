using System.Threading.Tasks;

namespace SkEditor.Contracts.Services.Files;

public interface IFilePickerService
{
    /// <summary>
    ///     Opens a file picker dialog and allows the user to select one or more files.
    /// </summary>
    /// <returns>The file paths of the selected files, or null if the operation was cancelled.</returns>
    Task<string[]?> PickFilesAsync();

    /// <summary>
    ///     Opens a save file dialog and allows the user to select a location and name for saving a file.
    /// </summary>
    /// <param name="fileName">The file name to display in the dialog title.</param>
    /// <returns>The file path of the selected location, or null if the operation was cancelled.</returns>
    Task<string?> SaveFileAsync(string fileName);
}