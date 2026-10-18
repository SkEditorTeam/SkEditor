using System.Threading.Tasks;

namespace SkEditor.Workspace.Session;

/// <summary>
///     Handles saving and restoring session state.
/// </summary>
public interface ISessionRestoreService
{
    /// <summary>
    ///     Saves the session state if session restore is enabled. Otherwise, deletes the existing session state file.
    /// </summary>
    Task SaveSessionStateAsync();

    /// <summary>
    ///     Restores the session state from a file if it exists and session restore is enabled.
    /// </summary>
    Task RecoverSessionStateAsync();
}