using System;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;

namespace SkEditor.Models.Projects.Sftp;

public class SftpFileSystem(
    RemoteConnection connection,
    IRemoteConnectionService connectionService,
    INotificationService notificationService,
    ILogService logService) : IFileSystem
{
    public SftpClient Client { get; private set; } = null!;
    public RemoteConnection Connection { get; } = connection;

    public FileSystemNode? Root { get; private set; }

    public async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            AuthenticationMethod authMethod = GetAuthMethod();
            ConnectionInfo info = new(Connection.Host, Connection.Port, Connection.Username, authMethod)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            Client = new SftpClient(info);
            Client.KeepAliveInterval = TimeSpan.FromSeconds(60);

            await Client.ConnectAsync(cancellationToken);

            if (!Client.IsConnected || cancellationToken.IsCancellationRequested)
            {
                notificationService.ShowError("Connection Failed", $"Failed to connect to {Connection.Name}");
                return false;
            }

            string workingDirectory = Client.WorkingDirectory;
            logService.Info($"Working directory: {workingDirectory}");

            Root = new SftpFolderNode(Connection.Name, Client.WorkingDirectory, true, Client) { IsRoot = true };
            await Root.LoadChildren();
            return true;
        }
        catch (Exception ex)
        {
            notificationService.ShowError("Connection Error", ex.Message);
            Client.Dispose();
            return false;
        }
    }

    private AuthenticationMethod GetAuthMethod()
    {
        AuthenticationMethod authMethod;

        if (Connection.AuthMethod == RemoteAuthMethod.Password)
        {
            string? password = connectionService.GetPassword(Connection);
            if (password == null)
            {
                logService.Error($"No password found for connection: {Connection.Name}");
                throw new InvalidOperationException("No password found for connection.");
            }

            authMethod = new PasswordAuthenticationMethod(Connection.Username, password);
        }
        else
        {
            authMethod = new PrivateKeyAuthenticationMethod(Connection.Username,
                new PrivateKeyFile(Connection.KeyFilePath));
        }

        return authMethod;
    }
}