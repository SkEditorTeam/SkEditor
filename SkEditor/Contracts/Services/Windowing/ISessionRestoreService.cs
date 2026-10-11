using System.Threading.Tasks;

namespace SkEditor.Contracts.Services.Windowing;

public interface ISessionRestoreService
{
    Task SaveSessionState();
    Task RecoverSessionState();
}