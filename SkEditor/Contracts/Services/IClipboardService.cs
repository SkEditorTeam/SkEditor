using System.Threading.Tasks;

namespace SkEditor.Contracts.Services;

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
}