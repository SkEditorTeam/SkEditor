using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentIcons.Common;

namespace SkEditor.Models.Projects.Local;

public class LocalFolderNode(string name, string path, bool isRoot) : BaseFolderNode(name, path, isRoot)
{
    protected override async Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders()
    {
        List<LocalFolderNode> folders = [];
        List<LocalFileNode> files = [];

        DirectoryInfo directory = new(Path);

        await Task.Run(() =>
        {
            foreach (FileSystemInfo entry in directory.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
            {
                if (IsSystemJunk(entry))
                {
                    continue;
                }

                switch (entry)
                {
                    case DirectoryInfo subDirectory:
                        folders.Add(new LocalFolderNode(subDirectory.Name, subDirectory.FullName, false));
                        break;
                    case FileInfo file:
                        files.Add(new LocalFileNode(file.Name, file.FullName));
                        break;
                }
            }
        });
        return folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Concat<FileSystemNode>(files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsSystemJunk(FileSystemInfo entry)
    {
        return entry.Attributes.HasFlag(FileAttributes.ReparsePoint) ||
               (entry.Attributes.HasFlag(FileAttributes.Hidden) &&
                entry.Attributes.HasFlag(FileAttributes.System));
    }

    public override List<ContextMenuItem> GetMenuItems()
    {
        if (IsRoot)
        {
            return
            [
                new ContextMenuItem { Header = "New File", Icon = Symbol.DocumentAdd, AllowForOneItemOnly = true },
                new ContextMenuItem { Header = "New Folder", Icon = Symbol.FolderAdd, AllowForOneItemOnly = true },
                ContextMenuItem.Separator,
                new ContextMenuItem { Header = "Copy Path", Icon = Symbol.Clipboard },
                ContextMenuItem.Separator,
                new ContextMenuItem { Header = "Show in Explorer", Icon = Symbol.FolderOpen },
                new ContextMenuItem { Header = "Open in Terminal", Icon = Symbol.CodeBlock }
            ];
        }

        return
        [
            new ContextMenuItem { Header = "New File", Icon = Symbol.DocumentAdd, AllowForOneItemOnly = true },
            new ContextMenuItem { Header = "New Folder", Icon = Symbol.FolderAdd, AllowForOneItemOnly = true },
            ContextMenuItem.Separator,
            new ContextMenuItem { Header = "Duplicate", Icon = Symbol.DocumentCopy },
            new ContextMenuItem { Header = "Rename", Icon = Symbol.Rename, AllowForOneItemOnly = true },
            new ContextMenuItem { Header = "Delete", Icon = Symbol.Delete },
            ContextMenuItem.Separator,
            new ContextMenuItem { Header = "Copy Path", Icon = Symbol.Clipboard },
            new ContextMenuItem { Header = "Copy Relative Path", Icon = Symbol.ClipboardLink },
            ContextMenuItem.Separator,
            new ContextMenuItem { Header = "Show in Explorer", Icon = Symbol.FolderOpen },
            new ContextMenuItem { Header = "Open in Terminal", Icon = Symbol.CodeBlock }
        ];
    }
}