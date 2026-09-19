using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Dialogs;
using SkEditor.Models.Dialogs.Elements;
using SkEditor.Models.Files;
using SkEditor.Models.Notifications;
using SkEditor.Models.Projects;
using SkEditor.Models.Projects.Local;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Projects.Types;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.ViewModels.Controls.FileTree;

public class FileTreeCommandHandler(
    FileTreeViewModel fileTree,
    AppSettings appSettings,
    IDialogService dialogs,
    IClipboardService clipboard,
    INotificationService notifs,
    ILogService logs,
    ITabService tabs,
    ILinkService links)
{
    public async Task NewFile()
    {
        if (fileTree.SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = fileTree.SelectedNodes[0];

        if (node is not BaseFolderNode folderNode ||
            !folderNode.GetSupportedActions().Contains(ProjectAction.NewFile))
        {
            return;
        }

        TextDialogElement inputElement = new()
        {
            PlaceholderText = TranslationKey.FileExplorerCreateNewEnterName,
            ValidationFunc = name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return TranslationKey.FileExplorerCreateNewNameEmpty;
                }

                if (name.IndexOfAny(node.InvalidChars) >= 0)
                {
                    return TranslationKey.FileExplorerCreateNewNameInvalidChars;
                }

                return folderNode.Children.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ? TranslationKey.FileExplorerCreateNewNameAlreadyExists
                    : null;
            }
        };

        Dialog dialog = Dialog
            .Create(TranslationKey.FileExplorerCreateNewFileTitle,
                TranslationKey.FileExplorerCreateNewFileMessage)
            .WithIcon(Symbol.DocumentAdd)
            .WithElements(inputElement)
            .WithButtons(new DialogButton(TranslationKey.ButtonConfirm, DialogResult.Ok, true), DialogButton.Cancel);

        DialogResult result = await dialogs.ShowDialogAsync(dialog);

        if (result == DialogResult.Ok)
        {
            await folderNode.NewFile(inputElement.Text);
        }
    }

    public async Task NewFolder()
    {
        if (fileTree.SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = fileTree.SelectedNodes[0];

        if (node is not BaseFolderNode folderNode ||
            !folderNode.GetSupportedActions().Contains(ProjectAction.NewFolder))
        {
            return;
        }

        TextDialogElement inputElement = new()
        {
            PlaceholderText = TranslationKey.FileExplorerCreateNewEnterName,
            ValidationFunc = name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return TranslationKey.FileExplorerCreateNewNameEmpty;
                }

                if (name.IndexOfAny(node.InvalidChars) >= 0)
                {
                    return TranslationKey.FileExplorerCreateNewNameInvalidChars;
                }

                return folderNode.Children.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ? TranslationKey.FileExplorerCreateNewNameAlreadyExists
                    : null;
            }
        };

        Dialog dialog = Dialog
            .Create(TranslationKey.FileExplorerCreateNewFolderTitle,
                TranslationKey.FileExplorerCreateNewFolderMessage)
            .WithIcon(Symbol.FolderAdd)
            .WithElements(inputElement)
            .WithButtons(new DialogButton(TranslationKey.ButtonConfirm, DialogResult.Ok, true), DialogButton.Cancel);

        DialogResult result = await dialogs.ShowDialogAsync(dialog);

        if (result == DialogResult.Ok)
        {
            await folderNode.NewFolder(inputElement.Text);
        }
    }

    public void CopyPath()
    {
        if (fileTree.SelectedNodes.Count == 0)
        {
            return;
        }

        string paths = string.Join("\n", fileTree.SelectedNodes.Select(n => FormatPath(n.Path)));
        clipboard.CopyTextAsync(paths);
    }

    public void CopyRelativePath()
    {
        if (fileTree.SelectedNodes.Count == 0)
        {
            return;
        }

        if (fileTree.FileSystem?.Root == null)
        {
            return;
        }

        string basePath = fileTree.FileSystem.Root.Path;

        IEnumerable<string> paths = fileTree.SelectedNodes
            .Select(n => n.Path.StartsWith(basePath) ? n.Path[basePath.Length..].TrimStart('/', '\\') : n.Path)
            .Select(FormatPath);

        string pathString = string.Join("\n", paths);

        clipboard.CopyTextAsync(pathString);
    }

    private string FormatPath(string path)
    {
        return OperatingSystem.IsWindows() && appSettings.IsExplorerForwardSlashesCopyEnabled
            ? path.Replace('\\', '/')
            : path;
    }

    public async Task Refresh()
    {
        if (fileTree.SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = fileTree.SelectedNodes[0];
        if (node is not BaseFolderNode folderNode)
        {
            return;
        }

        await folderNode.LoadChildrenAsync(true);
    }

    public async Task<bool> Disconnect()
    {
        if (tabs.AllTabs.OfType<IFileTab>().Any(t => t.File is SftpFile))
        {
            Dialog dialog = Dialog
                .Create(TranslationKey.RemoteConnectionDisconnectTitle)
                .WithMessage(TranslationKey.RemoteConnectionDisconnectMessage)
                .WithIcon(Symbol.PlugDisconnected)
                .WithButtons(new DialogButton(TranslationKey.ButtonDisconnect, DialogResult.Ok), DialogButton.Cancel);

            DialogResult result = await dialogs.ShowDialogAsync(dialog);
            if (result != DialogResult.Ok)
            {
                return false;
            }
        }

        if (fileTree.FileSystem is SftpFileSystem)
        {
            await fileTree.CloseFileSystemAsync();
        }

        return true;
    }

    public void CloseLocal()
    {
        if (fileTree.SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = fileTree.SelectedNodes[0];

        if (fileTree.FileSystem is LocalFileSystem && node.IsRoot)
        {
            _ = Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    await fileTree.CloseFileSystemAsync();
                }
                catch (Exception ex)
                {
                    logs.Error(ex, "Failed to close local file system");
                }
            }, DispatcherPriority.Background);
        }
    }

    public void Open()
    {
        foreach (FileSystemNode node in fileTree.SelectedNodes)
        {
            node.OpenCommand?.Execute(null);
        }
    }

    public void Rename()
    {
        if (fileTree.SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = fileTree.SelectedNodes[0];

        if (!node.GetSupportedActions().Contains(ProjectAction.Rename))
        {
            return;
        }

        node.IsRenaming = true;

        node.RenamedAsObservable()
            .Take(1)
            .ObserveOnUIThreadDispatcher()
            .SubscribeAwait(async (args, _) =>
            {
                try
                {
                    node.IsRenaming = false;

                    if (args.OldName == args.NewName)
                    {
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(args.NewName))
                    {
                        Notification notification = Notification
                            .Create(TranslationKey.ProjectRenameFailedTitle)
                            .WithMessage(TranslationKey.ProjectRenameErrorNameEmpty)
                            .WithType(NotificationType.Error);

                        notifs.Show(notification);

                        return;
                    }

                    await node.RenameAsync(args.NewName);
                }
                catch (Exception ex)
                {
                    logs.Error(ex, "Failed to rename node");

                    Notification notification = Notification
                        .Create(TranslationKey.ProjectRenameFailedTitle)
                        .WithMessage(TranslationKey.ProjectGeneralErrorDescription)
                        .WithType(NotificationType.Error);

                    notifs.Show(notification);
                }
            });
    }

    public async Task Delete()
    {
        if (fileTree.SelectedNodes.Any(n => n.IsRoot))
        {
            return;
        }

        TranslatableString message = fileTree.SelectedNodes.Count == 1
            ? new TranslatableString(TranslationKey.FileExplorerConfirmDeleteOneMessage, fileTree.SelectedNodes[0].Name)
            : new TranslatableString(TranslationKey.FileExplorerConfirmDeleteMultipleMessage,
                fileTree.SelectedNodes.Count);

        Dialog dialog = Dialog
            .Create(TranslationKey.FileExplorerConfirmDeleteTitle)
            .WithMessage(message)
            .WithIcon(Symbol.Delete)
            .WithButtons(new DialogButton(TranslationKey.ButtonDelete, DialogResult.Ok), DialogButton.Cancel);

        DialogResult result = await dialogs.ShowDialogAsync(dialog);

        if (result != DialogResult.Ok)
        {
            return;
        }

        List<FileSystemNode> nodesToDelete = fileTree.SelectedNodes.ToList();
        foreach (FileSystemNode node in nodesToDelete)
        {
            node.IsLoadingChildren = true;
            node.IsExpanded = false;
            bool isSuccess = await node.DeleteAsync();

            if (isSuccess)
            {
                node.OnDeleted();
            }

            node.IsLoadingChildren = false;
        }
    }

    public void ShowInExplorer()
    {
        FileSystemNode? node = fileTree.SelectedNodes.FirstOrDefault();
        if (node == null)
        {
            return;
        }

        if (node.IsDirectory)
        {
            if (!Directory.Exists(node.Path))
            {
                return;
            }

            links.OpenFolder(node.Path);
        }
        else
        {
            if (!File.Exists(node.Path))
            {
                return;
            }

            links.OpenFolderWithFileSelection(node.Path);
        }
    }
}