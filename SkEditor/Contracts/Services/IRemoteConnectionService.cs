using System.Threading.Tasks;
using SkEditor.Models.Remote;

namespace SkEditor.Contracts.Services;

public interface IRemoteConnectionService
{
    Task<RemoteConnectionTestResult> TestConnectionAsync(RemoteConnectionInfo connectionInfo);

    void SavePassword(RemoteConnection connection, string password);
    string? GetPassword(RemoteConnection connection);
}