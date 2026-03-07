using System;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using SkEditor.API;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;

namespace SkEditor.Models.Projects;

public class SftpFileSystem(RemoteConnection connection, IRemoteConnectionService connectionService) : IFileSystem
{
    public SftpClient Client { get; private set; }
    public RemoteConnection Connection { get; } = connection;

    public FileSystemNode Root { get; private set; }

    public async Task Load()
    {
        SkEditorAPI.Logs.Info(
            $"Will load SFTP file system for connection: {Connection.Name} ({Connection.Host}:{Connection.Port})");

        AuthenticationMethod authMethod = GetAuthMethod();
        ConnectionInfo info = new(Connection.Host, Connection.Port, Connection.Username, authMethod);

        Client = new SftpClient(info);
        using CancellationTokenSource cts = new();

        await Client.ConnectAsync(cts.Token);

        string workingDirectory = Client.WorkingDirectory;
        SkEditorAPI.Logs.Info($"Working directory: {workingDirectory}");

        Root = new SftpFolderNode(Connection.Name, Client.WorkingDirectory, true, Client);
        await Root.LoadChildren();
    }

    private AuthenticationMethod GetAuthMethod()
    {
        AuthenticationMethod authMethod;

        if (Connection.AuthMethod == RemoteAuthMethod.Password)
        {
            string? password = connectionService.GetPassword(Connection);
            if (password == null)
            {
                SkEditorAPI.Logs.Error($"No password found for connection: {Connection.Name}");
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