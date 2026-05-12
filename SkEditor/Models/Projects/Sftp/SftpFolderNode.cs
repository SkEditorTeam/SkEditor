using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Serilog;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Types;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Sftp;

public class SftpFolderNode(string name, string path, bool isRoot, SftpClient client)
    : BaseFolderNode(name, path, isRoot)
{
    protected override async Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders()
    {
        List<SftpFolderNode> folders = [];
        List<SftpFileNode> files = [];

        await foreach (ISftpFile file in client.ListDirectoryAsync(Path, CancellationToken.None))
        {
            if (file.IsDirectory)
            {
                if (file.Name is "." or "..")
                {
                    continue;
                }

                folders.Add(new SftpFolderNode(file.Name, file.FullName, false, client));
            }
            else if (file.IsRegularFile)
            {
                files.Add(new SftpFileNode(file.Name, file.FullName, client));
            }
        }

        return folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Concat<FileSystemNode>(files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase));
    }

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
            ProjectAction.CopyRelativePath
        ];
    }

    protected override Func<string, Task<bool>> AlreadyExistsPredicate =>
        async path => await client.ExistsAsync(path, CancellationToken.None);

    protected override Func<string, Task> CreateFileAction =>
        async path => await client.Create(path).DisposeAsync();

    protected override FileSystemNode CreateFileNode(string name, string path) =>
        new SftpFileNode(name, path, client);

    protected override Func<string, Task> CreateFolderAction =>
        async path => await client.CreateDirectoryAsync(path, CancellationToken.None);

    protected override Func<string, Task> DeleteAction =>
        async path => await client.DeleteDirectoryAsync(path, CancellationToken.None);

    protected override Func<string, string, Task> RenameFileAction =>
        async (oldPath, newPath) => await client.RenameFileAsync(oldPath, newPath, CancellationToken.None);

    protected override char[] InvalidChars { get; } = ['/', '\\', '\0'];

    protected override FileSystemNode CreateFolderNode(string name, string path) =>
        new SftpFolderNode(name, path, false, client);
}