using System.Threading.Tasks;
using SkEditor.Models.Dialogs;

namespace SkEditor.Contracts.Services;

/// <summary>
///     Service for displaying dialogs to the user.
/// </summary>
public interface IDialogService
{
    /// <summary>
    ///     Displays a dialog to the user and returns the result.
    /// </summary>
    /// <param name="dialog">The dialog to display.</param>
    /// <returns>The dialog result.</returns>
    Task<DialogResult> ShowDialogAsync(Dialog dialog);
}