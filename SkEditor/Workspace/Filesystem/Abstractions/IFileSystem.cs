using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Workspace.Filesystem.Abstractions;

/// <summary>
///     Represents a file system in a node-based structure
/// </summary>
public interface IFileSystem
{
    /// <summary>
    ///     The root node of the file system.
    /// </summary>
    FileSystemNode? Root { get; }

    /// <summary>
    ///     Loads the file system, populating the nodes with their children.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the loading process.</param>
    /// <returns>True if the file system was loaded successfully, false otherwise.</returns>
    Task<bool> LoadAsync(CancellationToken ct);
}