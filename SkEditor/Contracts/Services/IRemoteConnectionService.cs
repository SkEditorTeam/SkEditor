using System.Threading.Tasks;
using SkEditor.Models.Remote;

namespace SkEditor.Contracts.Services;

public interface IRemoteConnectionService
{
    Task<RemoteConnectionTestResult> TestConnection(RemoteConnectionInfo connectionInfo);
}