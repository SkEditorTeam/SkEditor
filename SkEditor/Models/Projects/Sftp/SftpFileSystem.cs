using System;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Sftp;

public class SftpFileSystem(
    RemoteConnection connection,
    IRemoteConnectionService connections,
    INotificationService notifications) : IFileSystem
{
    public FileSystemNode? Root { get; private set; }

    public async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            SftpClient client = await connections.GetOrConnectSftpClientAsync(connection, cancellationToken);

            Root = new SftpFolderNode(connection.Name, client.WorkingDirectory, true, client) { IsRoot = true };
            await Root.LoadChildren();
            return true;
        }
        catch (Exception ex)
        {
            notifications.ShowError(TranslatableString.FromRawString("Connection Error"),
                TranslatableString.FromRawString("Failed to connect to {0}: {1}", connection.Name, ex.Message));
            return false;
        }
    }
}