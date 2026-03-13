using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FluentIcons.Common;

namespace SkEditor.Models.Projects.Local;

public class LocalFolderNode(string name, string path, bool isRoot) : BaseFolderNode(name, path, isRoot)
{
    public override Symbol? IconSymbol => Symbol.Folder;

    public override IImmutableSolidColorBrush IconColor =>
        new ImmutableSolidColorBrush(Color.Parse("#FFCF40"));

    public override MenuFlyout ContextMenu => new()
    {
        Items =
        {
            new MenuItem { Header = "Hello" }
        }
    };

    public override async Task LoadChildren()
    {
        if (HasLoadedChildren)
        {
            return;
        }

        IsLoadingChildren = true;

        Children.Clear();

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

        foreach (FileSystemNode node in folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                     .Concat<FileSystemNode>(files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)))
        {
            Children.Add(node);
        }

        HasLoadedChildren = true;
        IsLoadingChildren = false;
    }

    private static bool IsSystemJunk(FileSystemInfo entry)
    {
        return entry.Attributes.HasFlag(FileAttributes.ReparsePoint) ||
               (entry.Attributes.HasFlag(FileAttributes.Hidden) &&
                entry.Attributes.HasFlag(FileAttributes.System));
    }
}