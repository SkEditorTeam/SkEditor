using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GitCredentialManager;
using Renci.SshNet;
using Renci.SshNet.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services;

public class RemoteConnectionService : IRemoteConnectionService
{
    private const string CredentialNamespace = "SkEditor Remote Connections";
    private const string CredentialTarget = "skeditor://sftp-connections";
    private readonly ICredentialStore _store = CredentialManager.Create(CredentialNamespace);

    public async Task<RemoteConnectionTestResult> TestConnection(RemoteConnectionInfo connectionInfo)
    {
        string keyFilePath = connectionInfo.KeyFilePath;

        bool invalidKeyFile = connectionInfo.IsPrivateKeyAuth &&
                              (string.IsNullOrWhiteSpace(keyFilePath) || !File.Exists(keyFilePath));
        if (invalidKeyFile)
        {
            return RemoteConnectionTestResult.Error(Translation.Get(TranslationKey.RemoteConnectionInvalidKeyPath));
        }

        try
        {
            Stopwatch sw = Stopwatch.StartNew();
            bool connected = await TryConnect(connectionInfo);
            sw.Stop();

            if (connected)
            {
                return RemoteConnectionTestResult.Success(Translation.Get(TranslationKey.RemoteConnectionTestSuccess,
                    connectionInfo.Host,
                    $"{sw.ElapsedMilliseconds:N0}"));
            }

            return RemoteConnectionTestResult.Error(
                Translation.Get(TranslationKey.RemoteConnectionTestFailed, connectionInfo.Host));
        }
        catch (SshException ex) when (ex.Message == "Invalid private key file.")
        {
            return RemoteConnectionTestResult.Error(Translation.Get(TranslationKey.RemoteConnectionInvalidKeyFormat));
        }
        catch (Exception ex)
        {
            return RemoteConnectionTestResult.Error(ex.Message);
        }
    }

    public void SavePassword(RemoteConnection connection, string password)
    {
        _store.AddOrUpdate(CredentialTarget, connection.Id.ToString(), password);
    }

    public string? GetPassword(RemoteConnection connection)
    {
        ICredential? cred = _store.Get(CredentialTarget, connection.Id.ToString());
        return cred.Password;
    }

    private static async Task<bool> TryConnect(RemoteConnectionInfo connectionInfo)
    {
        int port = int.TryParse(connectionInfo.Port, out int p) ? p : 22;

        AuthenticationMethod authMethod = connectionInfo.IsPasswordAuth
            ? new PasswordAuthenticationMethod(connectionInfo.Username, connectionInfo.Password)
            : new PrivateKeyAuthenticationMethod(connectionInfo.Username,
                new PrivateKeyFile(connectionInfo.KeyFilePath));

        var info = new ConnectionInfo(connectionInfo.Host, port, connectionInfo.Username, authMethod);

        using var client = new SftpClient(info);
        using var cts = new CancellationTokenSource();

        await client.ConnectAsync(cts.Token);

        return client.IsConnected;
    }
}