namespace SkEditor.Contracts.Services;

public interface ILinkService
{
    /// <summary>
    ///     Opens the specified URL in the default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    void OpenLink(string url);

    /// <summary>
    ///     Opens the specified folder in the system's file explorer.
    /// </summary>
    /// <param name="path">The path of the folder to open.</param>
    void OpenFolder(string path);

    /// <summary>
    ///     Opens the folder containing the specified file and selects the file in the file explorer.
    /// </summary>
    /// <param name="filePath"></param>
    /// <remarks>Supported on Windows and macOS. On Linux, it will open the folder but not select the file.</remarks>
    void OpenFolderWithFileSelection(string filePath);
}