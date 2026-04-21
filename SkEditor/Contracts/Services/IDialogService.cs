using System.Threading.Tasks;
using SkEditor.Models.Dialogs;

namespace SkEditor.Contracts.Services;

public interface IDialogService
{
    Task<DialogResult> ShowDialog(Dialog dialog);
}