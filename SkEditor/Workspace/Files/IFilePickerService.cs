using System.Threading.Tasks;
using SkEditor.Models.Files;
using SkEditor.Utilities.Localization;

namespace SkEditor.Contracts.Services.Files;

public interface IFilePickerService
{
    /// <summary>
    ///     Opens a file picker dialog and allows the user to select one or more files.
    /// </summary>
    /// <param name="title">The title of the file picker dialog.</param>
    /// <param name="allowMultiple">Whether to allow selecting multiple files.</param>
    /// <param name="fileTypes">Filter for the types of files to display in the dialog.</param>
    /// <returns>The file paths of the selected files, or null if the operation was cancelled.</returns>
    Task<string[]?> PickFilesAsync(TranslatableString? title = null, bool allowMultiple = true,
        params FilePickerType[] fileTypes);

    /// <summary>
    ///     Opens a folder picker dialog and allows the user to select one or more folders.
    /// </summary>
    /// <param name="allowMultiple">Whether to allow selecting multiple folders.</param>
    /// <returns>The folder paths of the selected folders, or null if the operation was cancelled.</returns>
    Task<string[]?> PickFoldersAsync(bool allowMultiple = true);

    /// <summary>
    ///     Opens a save file dialog and allows the user to select a location and name for saving a file.
    /// </summary>
    /// <param name="fileName">The file name to display in the dialog title.</param>
    /// <param name="currentPath">The current file path.</param>
    /// <returns>The file path of the selected location, or null if the operation was cancelled.</returns>
    Task<string?> SaveFileAsync(string fileName, string? currentPath = null);
}