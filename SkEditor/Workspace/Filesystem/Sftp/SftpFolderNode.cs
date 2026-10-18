using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.Projects.Models;
using SkEditor.Workspace.Remote;
using SkEditor.Workspace.Remote.Models;

namespace SkEditor.Workspace.Filesystem.Sftp;

public class SftpFolderNode(
    string name,
    string path,
    bool isRoot,
    RemoteConnection connection,
    IRemoteConnectionService service,
    bool addDummy = true)
    : BaseFolderNode(name, path, isRoot, addDummy)
{
    public override char[] InvalidChars => SftpFileSystem.InvalidChars;

    protected override Func<string, Task<bool>> AlreadyExistsPredicate =>
        SftpFileSystem.GetAlreadyExistsPredicate(service, connection);

    protected override Func<string, Task> CreateFileAction =>
        async path => await service.ExecuteSafeAsync(connection,
            async c => await c.Create(path).DisposeAsync());

    protected override Func<string, Task> CreateFolderAction =>
        async path => await service.ExecuteSafeAsync(connection,
            async c => await c.CreateDirectoryAsync(path));

    protected override Func<string, Task> DeleteAction =>
        async path => await service.ExecuteSafeAsync(connection,
            async c => { await DeleteDirectoryRecursiveAsync(c, path, CancellationToken.None); });

    protected override Func<string, string, Task> RenameFileAction =>
        async (oldPath, newPath) => await service.ExecuteSafeAsync(connection,
            async c => await c.RenameFileAsync(oldPath, newPath, CancellationToken.None));

    public override IEnumerable<ProjectAction> GetSupportedActions()
    {
        if (IsRoot)
        {
            return
            [
                ProjectAction.NewFile,
                ProjectAction.NewFolder,
                ProjectAction.Separator,
                ProjectAction.Upload,
                ProjectAction.Download,
                ProjectAction.Separator,
                ProjectAction.CopyPath,
                ProjectAction.Separator,
                ProjectAction.Refresh,
                ProjectAction.Disconnect
            ];
        }

        return
        [
            ProjectAction.NewFile,
            ProjectAction.NewFolder,
            ProjectAction.Separator,
            ProjectAction.Upload,
            ProjectAction.Download,
            ProjectAction.Separator,
            ProjectAction.Duplicate,
            ProjectAction.Rename,
            ProjectAction.Delete,
            ProjectAction.Separator,
            ProjectAction.CopyPath,
            ProjectAction.CopyRelativePath,
            ProjectAction.Separator,
            ProjectAction.Refresh
        ];
    }

    protected override async Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders()
    {
        List<SftpFolderNode> folders = [];
        List<SftpFileNode> files = [];

        try
        {
            List<ISftpFile> remoteFiles = await service.ExecuteSafeAsync(connection, async c =>
            {
                List<ISftpFile> list = [];
                await foreach (ISftpFile file in c.ListDirectoryAsync(Path, CancellationToken.None))
                {
                    list.Add(file);
                }

                return list;
            });

            foreach (ISftpFile file in remoteFiles)
            {
                if (file.IsDirectory)
                {
                    if (file.Name is "." or "..")
                    {
                        continue;
                    }

                    folders.Add(new SftpFolderNode(file.Name, file.FullName, false, connection, service));
                }
                else if (file.IsRegularFile)
                {
                    files.Add(new SftpFileNode(file.Name, file.FullName, connection, service));
                }
            }
        }
        catch (SftpPermissionDeniedException)
        {
            throw new UnauthorizedAccessException($"You do not have permission to access the folder '{Path}'.");
        }

        return folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Concat<FileSystemNode>(files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase));
    }

    protected override string CombinePath(string folderPath, string itemName)
    {
        return $"{folderPath.TrimEnd('/')}/{itemName}";
    }

    protected override string GetDirectoryName(string path)
    {
        int lastSlash = path.LastIndexOf('/');
        return lastSlash <= 0 ? "/" : path[..lastSlash];
    }

    protected override FileSystemNode CreateFileNode(string name, string path)
    {
        return new SftpFileNode(name, path, connection, service);
    }

    protected override FileSystemNode CreateFolderNode(string name, string path, bool addDummy = true)
    {
        return new SftpFolderNode(name, path, false, connection, service, addDummy);
    }

    public async Task DeleteDirectoryRecursiveAsync(SftpClient client, string path, CancellationToken ct = default)
    {
        IAsyncEnumerable<ISftpFile> files = client.ListDirectoryAsync(path, ct);

        await foreach (ISftpFile file in files)
        {
            if (file.Name is "." or "..")
            {
                continue;
            }

            if (file.IsDirectory)
            {
                await DeleteDirectoryRecursiveAsync(client, file.FullName, ct);
            }
            else
            {
                await client.DeleteFileAsync(file.FullName, ct);
            }
        }

        await client.DeleteDirectoryAsync(path, ct);
    }
}