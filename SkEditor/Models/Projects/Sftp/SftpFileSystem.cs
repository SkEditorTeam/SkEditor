using System;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Notifications;
using SkEditor.Models.Projects.Types;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Sftp;

public class SftpFileSystem(
    RemoteConnection connection,
    IRemoteConnectionService connections,
    INotificationService notifications) : IFileSystem
{
    public static readonly char[] InvalidChars = ['/', '\\', '\0'];
    public FileSystemNode? Root { get; private set; }
    public RemoteConnection Connection => connection;

    public async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            SftpClient client = await connections.GetOrConnectSftpClientAsync(connection, cancellationToken);

            string rootPath = string.IsNullOrWhiteSpace(connection.CustomRootPath)
                ? client.WorkingDirectory
                : connection.CustomRootPath ?? client.WorkingDirectory;

            bool rootPathExists = await connections.ExecuteSafeAsync(connection,
                async c => await c.ExistsAsync(rootPath, cancellationToken), cancellationToken);

            if (!rootPathExists)
            {
                Notification notification = Notification
                    .Create(TranslationKey.RemoteRootPathNotFoundTitle)
                    .WithMessage(TranslationKey.RemoteRootPathNotFoundMessage)
                    .WithType(NotificationType.Error);

                notifications.Show(notification);
                return false;
            }

            Root = new SftpFolderNode(connection.Name, rootPath, true, connection, connections)
                { IsRoot = true };

            await Root.LoadChildrenAsync();
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Notification notification = Notification
                .Create(TranslationKey.RemoteConnectionErrorTitle)
                .WithMessage(new TranslatableString(TranslationKey.RemoteConnectionErrorMessage, connection.Name,
                    ex.Message))
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
    }

    public void Disconnect()
    {
        if (Root is not SftpFolderNode)
        {
            return;
        }

        SftpClient? client = connections.GetSftpClient(connection);
        client?.Disconnect();

        connections.Disconnect(connection);

        Root = null;
    }

    public static Func<string, Task<bool>> GetAlreadyExistsPredicate(IRemoteConnectionService service,
        RemoteConnection connection)
    {
        return async path =>
        {
            try
            {
                return await service.ExecuteSafeAsync(connection,
                    async c => await c.ExistsAsync(path, CancellationToken.None));
            }
            catch (Exception)
            {
                return false;
            }
        };
    }
}