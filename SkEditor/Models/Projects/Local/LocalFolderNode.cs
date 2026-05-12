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

    public override async Task NewFile(string name)
    {
        string newFilePath = System.IO.Path.Combine(Path, name);

        if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("File Creation Failed"),
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

            return;
        }

        try
        {
            await File.Create(newFilePath).DisposeAsync();

            FileSystemNode newNode = new LocalFileNode(name, newFilePath);
            AddChild(newNode);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("File Creation Failed"),
                TranslatableString.FromRawString(ex.Message), NotificationType.Error));

            Log.Error(ex, "Failed to create file {FilePath}", newFilePath);
        }
    }

    public override Task NewFolder(string name)
    {
        string newFolderPath = System.IO.Path.Combine(Path, name);

        if (File.Exists(newFolderPath) || Directory.Exists(newFolderPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("Folder Creation Failed"),
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

            return Task.CompletedTask;
        }

        try
        {
            Directory.CreateDirectory(newFolderPath);

            FileSystemNode newNode = new LocalFolderNode(name, newFolderPath, false);
            AddChild(newNode);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("Folder Creation Failed"),
                TranslatableString.FromRawString(ex.Message), NotificationType.Error));
        }
        
        return Task.CompletedTask;
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

    public override Task Rename(string newName)
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
}