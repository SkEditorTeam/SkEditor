using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentIcons.Common;
using Renci.SshNet;
using Renci.SshNet.Sftp;

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

    public override List<ContextMenuItem> GetMenuItems()
    {
        if (IsRoot)
        {
            return
            [
                new ContextMenuItem { Header = "New File", Icon = Symbol.DocumentAdd },
                new ContextMenuItem { Header = "New Folder", Icon = Symbol.FolderAdd },
                ContextMenuItem.Separator,
                new ContextMenuItem { Header = "Copy Path", Icon = Symbol.Clipboard },
                ContextMenuItem.Separator,
                new ContextMenuItem { Header = "Refresh", Icon = Symbol.ArrowClockwise },
                new ContextMenuItem { Header = "Disconnect", Icon = Symbol.PlugDisconnected }
            ];
        }

        return
        [
            new ContextMenuItem { Header = "New File", Icon = Symbol.DocumentAdd },
            new ContextMenuItem { Header = "New Folder", Icon = Symbol.FolderAdd },
            ContextMenuItem.Separator,
            new ContextMenuItem { Header = "Duplicate", Icon = Symbol.DocumentCopy },
            new ContextMenuItem { Header = "Rename", Icon = Symbol.Rename },
            new ContextMenuItem { Header = "Delete", Icon = Symbol.Delete },
            ContextMenuItem.Separator,
            new ContextMenuItem { Header = "Copy Path", Icon = Symbol.Clipboard },
            new ContextMenuItem { Header = "Copy Relative Path", Icon = Symbol.ClipboardLink }
        ];
    }
}