using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkEditor.Models.Projects.Types;

namespace SkEditor.Models.Projects.Local;

public class LocalFolderNode(string name, string path, bool isRoot) : BaseFolderNode(name, path, isRoot)
{
    protected override Func<string, Task<bool>> AlreadyExistsPredicate =>
        path => Task.FromResult(File.Exists(path) || Directory.Exists(path));

    protected override Func<string, Task> CreateFileAction =>
        async path => await File.Create(path).DisposeAsync();

    protected override Func<string, Task> CreateFolderAction =>
        async path => await Task.Run(() => Directory.CreateDirectory(path));

    protected override Func<string, Task> DeleteAction =>
        async path => await Task.Run(() => Directory.Delete(path, true));

    protected override Func<string, string, Task> RenameFileAction =>
        async (oldPath, newPath) => await Task.Run(() => Directory.Move(oldPath, newPath));

    protected override char[] InvalidChars { get; } = System.IO.Path.GetInvalidFileNameChars();

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
                    {
                        LocalFolderNode folderNode = new(subDirectory.Name, subDirectory.FullName, false);

                        if (!HasVisibleContents(subDirectory.FullName))
                        {
                            folderNode.Children.Clear();
                        }

                        folders.Add(folderNode);
                        break;
                    }
                    case FileInfo file:
                    {
                        files.Add(new LocalFileNode(file.Name, file.FullName));
                        break;
                    }
                }
            }
        });
        return folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Concat<FileSystemNode>(files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase));
    }

    private static bool HasVisibleContents(string path)
    {
        try
        {
            return new DirectoryInfo(path)
                .EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly)
                .Any(entry => !IsSystemJunk(entry));
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsSystemJunk(FileSystemInfo entry)
    {
        return entry.Attributes.HasFlag(FileAttributes.ReparsePoint) ||
               (entry.Attributes.HasFlag(FileAttributes.Hidden) &&
                entry.Attributes.HasFlag(FileAttributes.System));
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
                ProjectAction.CopyPath,
                ProjectAction.ShowInExplorer,
                ProjectAction.Separator,
                ProjectAction.Close
            ];
        }

        return
        [
            ProjectAction.NewFile,
            ProjectAction.NewFolder,
            ProjectAction.Separator,
            ProjectAction.Duplicate,
            ProjectAction.Rename,
            ProjectAction.Delete,
            ProjectAction.Separator,
            ProjectAction.CopyPath,
            ProjectAction.CopyRelativePath,
            ProjectAction.Separator,
            ProjectAction.ShowInExplorer
        ];
    }

    protected override FileSystemNode CreateFileNode(string name, string path)
    {
        return new LocalFileNode(name, path);
    }

    protected override FileSystemNode CreateFolderNode(string name, string path)
    {
        return new LocalFolderNode(name, path, false);
    }
}