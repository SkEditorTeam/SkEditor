using System;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using SkEditor.Models.Remote;

namespace SkEditor.Contracts.Services;

public interface IRemoteConnectionService
{
    Task<RemoteConnectionTestResult> TestConnectionAsync(RemoteConnectionInfo connectionInfo);

    void SavePassword(RemoteConnection connection, string password);
    string? GetPassword(RemoteConnection connection);

    Task<SftpClient> GetOrConnectSftpClientAsync(RemoteConnection connection,
        CancellationToken cancellationToken = default);

    SftpClient? GetSftpClient(RemoteConnection connection);

    void Disconnect(RemoteConnection connection);

    Task ExecuteSafeAsync(RemoteConnection connection, Func<SftpClient, Task> action,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteSafeAsync<T>(RemoteConnection connection, Func<SftpClient, Task<T>> action,
        CancellationToken cancellationToken = default);
}