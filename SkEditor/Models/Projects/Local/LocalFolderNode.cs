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
                ContextMenuItem.Item("New File").WithIcon(Symbol.DocumentAdd).AllowOneItemOnly(),
                ContextMenuItem.Item("New Folder").WithIcon(Symbol.FolderAdd).AllowOneItemOnly(),
                ContextMenuItem.Separator,
                ContextMenuItem.Item("Copy Path").WithIcon(Symbol.Clipboard),
                ContextMenuItem.Separator,
                ContextMenuItem.Item("Show in Explorer").WithIcon(Symbol.FolderOpen),
                ContextMenuItem.Item("Open in Terminal").WithIcon(Symbol.CodeBlock).AllowOneItemOnly()
            ];
        }

        return
        [
            ContextMenuItem.Item("New File").WithIcon(Symbol.DocumentAdd).AllowOneItemOnly(),
            ContextMenuItem.Item("New Folder").WithIcon(Symbol.FolderAdd).AllowOneItemOnly(),
            ContextMenuItem.Separator,
            .. GetCommonMenuItems(),
            ContextMenuItem.Separator,
            ContextMenuItem.Item("Show in Explorer").WithIcon(Symbol.FolderOpen),
            ContextMenuItem.Item("Open in Terminal").WithIcon(Symbol.CodeBlock).AllowOneItemOnly()
        ];
    }
}