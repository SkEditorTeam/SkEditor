using System.Threading.Tasks;

namespace SkEditor.Core.System;

/// <summary>
///     Used for interacting with the system clipboard.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    ///     Copies the specified text to the system clipboard.
    /// </summary>
    /// <param name="text">The text to copy to the clipboard.</param>
    Task CopyTextAsync(string text);

    /// <summary>
    ///     Retrieves text from the system clipboard, if available.
    /// </summary>
    /// <returns>The text from the clipboard, or null if no text is available.</returns>
    Task<string?> GetTextAsync();
}