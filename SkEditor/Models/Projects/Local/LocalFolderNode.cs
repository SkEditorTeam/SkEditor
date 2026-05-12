using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Types;
using SkEditor.Utilities.Localization;

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
                    {
                        var folderNode = new LocalFolderNode(subDirectory.Name, subDirectory.FullName, false);

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

    public override Task RenameAsync(string newName)
    {
        if (IsRoot || System.IO.Path.GetDirectoryName(Path) is not { } parentDirectory) return Task.CompletedTask;

        if (newName.IndexOfAny(LocalFileNode.InvalidChars) >= 0)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectRenameErrorNameInvalid, NotificationType.Error));

            return Task.CompletedTask;
        }

        string newPath = System.IO.Path.Combine(parentDirectory, newName);

        if (Directory.Exists(newPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

            return Task.CompletedTask;
        }

        try
        {
            Directory.Move(Path, newPath);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle, TranslatableString.FromRawString(ex.Message),
                NotificationType.Error));

            Log.Error(ex, "Failed to rename folder {Path} to {NewName}", Path, newName);
            return Task.CompletedTask;
        }

        Path = newPath;
        Name = newName;

        return Task.CompletedTask;
    }

    public override Task<bool> DeleteAsync()
    {
        try
        {
            Directory.Delete(Path, true);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("Folder Delete Failed"),
                TranslatableString.FromRawString(ex.Message),
                NotificationType.Error));

            Log.Error(ex, "Failed to delete folder {Path}", Path);
            return Task.FromResult(false);
        }
    }
    
    protected override Func<string, Task<bool>> AlreadyExistsPredicate =>
        path => Task.FromResult(File.Exists(path) || Directory.Exists(path));
    
    protected override Func<string, Task> CreateFileAction =>
        async path => await File.Create(path).DisposeAsync();
    
    protected override FileSystemNode CreateFileNode(string name, string path) =>
        new LocalFileNode(name, path);
    
    protected override Func<string, Task> CreateFolderAction =>
        async path => await Task.Run(() => Directory.CreateDirectory(path));
    
    protected override FileSystemNode CreateFolderNode(string name, string path) =>
        new LocalFolderNode(name, path, false);
}