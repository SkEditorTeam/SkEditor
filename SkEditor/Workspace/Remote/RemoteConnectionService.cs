using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Renci.SshNet;
using Renci.SshNet.Common;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.Security;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Filesystem.Sftp;
using SkEditor.Workspace.Remote.Messages;
using SkEditor.Workspace.Remote.Models;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Workspace.Remote;

public class RemoteConnectionService(
    ITranslationService translation,
    ITabService tabs,
    ILogService logs,
    INotificationService notifications,
    ICredentialStoreFactory credentialStoreFactory)
    : IRemoteConnectionService, IDisposable
{
    private readonly Dictionary<RemoteConnection, SftpClient> _activeConnections = new();
    private readonly ConcurrentDictionary<RemoteConnection, SemaphoreSlim> _connectionLocks = new();
    private readonly ICredentialStore _store = credentialStoreFactory.Create();

    public async Task<RemoteConnectionTestResult> TestConnectionAsync(RemoteConnectionInfo connectionInfo)
    {
        string keyFilePath = connectionInfo.KeyFilePath;

        bool invalidKeyFile = connectionInfo.IsPrivateKeyAuth &&
                              (string.IsNullOrWhiteSpace(keyFilePath) || !File.Exists(keyFilePath));
        if (invalidKeyFile)
        {
            return RemoteConnectionTestResult.Error(translation.Get(TranslationKey.RemoteConnectionInvalidKeyPath));
        }

        try
        {
            RemoteConnectionInfo resolvedInfo = GetConnectionInfoForTesting(connectionInfo);

            Stopwatch sw = Stopwatch.StartNew();
            (bool isConnected, bool rootExists) = await TryConnect(resolvedInfo);
            sw.Stop();

            if (!isConnected)
            {
                return RemoteConnectionTestResult.Error(
                    translation.Get(TranslationKey.RemoteConnectionTestFailed, connectionInfo.Host));
            }

            if (!rootExists)
            {
                return RemoteConnectionTestResult.Error(translation.Get(TranslationKey.RemoteRootPathNotFoundMessage));
            }

            return RemoteConnectionTestResult.Success(translation.Get(TranslationKey.RemoteConnectionTestSuccess,
                connectionInfo.Host,
                $"{sw.ElapsedMilliseconds:N0}"));
        }
        catch (SshException ex) when (ex.Message == "Invalid private key file.")
        {
            return RemoteConnectionTestResult.Error(translation.Get(TranslationKey.RemoteConnectionInvalidKeyFormat));
        }
        catch (Exception ex)
        {
            return RemoteConnectionTestResult.Error(ex.Message);
        }
    }

    public void SavePassword(RemoteConnection connection, string password)
    {
        string hash = _store.Protect(password);
        connection.PasswordHash = hash;
    }

    public string? GetPassword(RemoteConnection connection)
    {
        if (string.IsNullOrEmpty(connection.PasswordHash))
        {
            return null;
        }

        try
        {
            return _store.Unprotect(connection.PasswordHash);
        }
        catch (Exception ex)
        {
            logs.Error($"Error retrieving password for connection {connection.Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<SftpClient> GetOrConnectSftpClientAsync(RemoteConnection connection,
        CancellationToken cancellationToken = default)
    {
        SemaphoreSlim connectionLock = _connectionLocks.GetOrAdd(connection, _ => new SemaphoreSlim(1, 1));

        await connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_activeConnections.TryGetValue(connection, out SftpClient? existingClient))
            {
                if (existingClient.IsConnected)
                {
                    return existingClient;
                }

                existingClient.Dispose();
                _activeConnections.Remove(connection);
            }

            AuthenticationMethod authMethod = GetAuthMethod(connection);
            ConnectionInfo info = new(connection.Host, connection.Port, connection.Username, authMethod)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            SftpClient newClient = new(info)
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60)
            };

            await newClient.ConnectAsync(cancellationToken);

            _activeConnections[connection] = newClient;

            return newClient;
        }
        finally
        {
            connectionLock.Release();
        }
    }

    public SftpClient? GetSftpClient(RemoteConnection connection)
    {
        if (_activeConnections.TryGetValue(connection, out SftpClient? client) && client.IsConnected)
        {
            return client;
        }

        return null;
    }

    public void Disconnect(RemoteConnection connection)
    {
        if (_activeConnections.Remove(connection, out SftpClient? client))
        {
            try
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                logs.Error($"Error disconnecting from {connection.Name}: {ex.Message}");
            }
            finally
            {
                client.Dispose();
            }
        }

        CloseConnectionTabs(connection);
        WeakReferenceMessenger.Default.Send(
            new RemoteConnectionStateChangedMessage(connection, RemoteConnectionState.Disconnected));
    }

    public async Task ExecuteSafeAsync(RemoteConnection connection, Func<SftpClient, Task> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            SftpClient client = await GetOrConnectSftpClientAsync(connection, cancellationToken);

            if (!client.IsConnected)
            {
                await client.ConnectAsync(cancellationToken);
            }

            await action(client);
        }
        catch (Exception ex) when (ex is IOException or SshConnectionException or SocketException)
        {
            logs.Warning($"Sftp connection lost for {connection.Name}. Attempting to reconnect...");

            WeakReferenceMessenger.Default.Send(
                new RemoteConnectionStateChangedMessage(connection, RemoteConnectionState.Reconnecting));

            try
            {
                ResetClient(connection);
                SftpClient client = await GetOrConnectSftpClientAsync(connection, cancellationToken);

                await action(client);
                WeakReferenceMessenger.Default.Send(
                    new RemoteConnectionStateChangedMessage(connection, RemoteConnectionState.Connected));
            }
            catch (Exception retryEx)
            {
                logs.Error($"Failed to reconnect and execute operation for {connection.Name}: {retryEx.Message}");

                notifications.Show(Notification
                    .Create(TranslationKey.RemoteConnectionLostConnectionTitle)
                    .WithMessage(new TranslatableString(TranslationKey.RemoteConnectionLostConnectionMessage,
                        connection.Name))
                    .WithType(NotificationType.Error));

                Disconnect(connection);
                throw;
            }
        }
    }

    public async Task<T> ExecuteSafeAsync<T>(RemoteConnection connection, Func<SftpClient, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        T result = default!;
        await ExecuteSafeAsync(connection, async client => { result = await action(client); }, cancellationToken);
        return result;
    }

    private RemoteConnectionInfo GetConnectionInfoForTesting(RemoteConnectionInfo connectionInfo)
    {
        RemoteConnectionInfo testInfo = new()
        {
            Name = connectionInfo.Name,
            Host = connectionInfo.Host,
            Port = connectionInfo.Port,
            Username = connectionInfo.Username,
            AuthMethod = connectionInfo.AuthMethod,
            KeyFilePath = connectionInfo.KeyFilePath,
            Password = connectionInfo.Password,
            CustomRootPath = connectionInfo.CustomRootPath
        };

        if (!testInfo.IsPasswordAuth || !string.IsNullOrEmpty(testInfo.Password)
                                     || !connectionInfo.IsPasswordSaved ||
                                     string.IsNullOrEmpty(connectionInfo.PasswordHash))
        {
            return testInfo;
        }

        try
        {
            testInfo.Password = _store.Unprotect(connectionInfo.PasswordHash);
        }
        catch (Exception ex)
        {
            logs.Error($"Error decrypting password for testing: {ex.Message}");
        }

        return testInfo;
    }

    private void CloseConnectionTabs(RemoteConnection connection)
    {
        List<FileTabViewModel> openTabs = tabs.AllTabs.OfType<FileTabViewModel>()
            .Where(tab => tab.File is SftpFile sftpFile && sftpFile.Connection == connection)
            .ToList();

        foreach (FileTabViewModel tab in openTabs)
        {
            tabs.CloseTab(tab);
        }
    }

    private AuthenticationMethod GetAuthMethod(RemoteConnection connection)
    {
        AuthenticationMethod authMethod;

        if (connection.AuthMethod == RemoteAuthMethod.Password)
        {
            string? password = GetPassword(connection);
            if (password == null)
            {
                logs.Error($"No password found for connection: {connection.Name}");
                throw new InvalidOperationException("No password found for connection.");
            }

            authMethod = new PasswordAuthenticationMethod(connection.Username, password);
        }
        else
        {
            authMethod = new PrivateKeyAuthenticationMethod(connection.Username,
                new PrivateKeyFile(connection.KeyFilePath));
        }

        return authMethod;
    }

    private static async Task<(bool isConnected, bool rootExists)> TryConnect(RemoteConnectionInfo connectionInfo)
    {
        int port = int.TryParse(connectionInfo.Port, out int p) ? p : 22;

        AuthenticationMethod authMethod = connectionInfo.IsPasswordAuth
            ? new PasswordAuthenticationMethod(connectionInfo.Username, connectionInfo.Password)
            : new PrivateKeyAuthenticationMethod(connectionInfo.Username,
                new PrivateKeyFile(connectionInfo.KeyFilePath));

        ConnectionInfo info = new(connectionInfo.Host, port, connectionInfo.Username, authMethod);

        using SftpClient client = new(info);
        using CancellationTokenSource cts = new();

        await client.ConnectAsync(cts.Token);

        string rootPath = string.IsNullOrWhiteSpace(connectionInfo.CustomRootPath)
            ? client.WorkingDirectory
            : connectionInfo.CustomRootPath ?? client.WorkingDirectory;

        bool rootPathExists = await client.ExistsAsync(rootPath, cts.Token);

        return (client.IsConnected, rootPathExists);
    }

    private void ResetClient(RemoteConnection connection)
    {
        if (!_activeConnections.Remove(connection, out SftpClient? client))
        {
            return;
        }

        try
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            client.Dispose();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (SftpClient client in _activeConnections.Values)
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }

            client.Dispose();
        }

        _activeConnections.Clear();
        foreach (SemaphoreSlim semaphore in _connectionLocks.Values)
        {
            semaphore.Dispose();
        }

        _connectionLocks.Clear();
    }
}