using System.Threading.Tasks;

namespace SkEditor.Contracts.Services;

public interface IDialogService
{
    Task Debug(string message);
}