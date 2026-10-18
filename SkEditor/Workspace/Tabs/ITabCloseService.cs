using System.Threading.Tasks;
using SkEditor.Models.Tabs;

namespace SkEditor.Contracts.Services.Files;

/// <summary>
///     Used to close multiple tabs at once, such as "Close All", "Close Others", etc.
/// </summary>
public interface ITabCloseService
{
    /// <summary>
    ///     Closes multiple tabs based on the given close type.
    /// </summary>
    /// <param name="closeType">The type of closing to perform.</param>
    Task CloseMultipleAsync(CloseType closeType);
}