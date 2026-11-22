using System.Threading.Tasks;

namespace SkEditor.Core.System;

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
    /// <param name="filePath">The path of the file.</param>
    /// <remarks>Supported on Windows, macOS and Linux. Fallbacks to opening the folder only.</remarks>
    ValueTask OpenFolderWithFileSelectionAsync(string filePath);
}