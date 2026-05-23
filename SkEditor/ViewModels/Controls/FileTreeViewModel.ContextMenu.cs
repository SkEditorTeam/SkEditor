using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Models.Dialogs;
using SkEditor.Models.Dialogs.Elements;
using SkEditor.Models.Notifications;
using SkEditor.Models.Projects;
using SkEditor.Models.Projects.Local;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Projects.Types;
using SkEditor.Utilities;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel
{
    private Dictionary<ProjectAction, IRelayCommand> _commandMap;

    [MemberNotNull(nameof(_commandMap))]
    private void InitializeCommandMap()
    {
        _commandMap = new Dictionary<ProjectAction, IRelayCommand>
        {
            { ProjectAction.Open, OpenCommand },
            { ProjectAction.NewFile, NewFileCommand },
            { ProjectAction.NewFolder, NewFolderCommand },
            { ProjectAction.Rename, RenameCommand },
            { ProjectAction.Delete, DeleteCommand },
            { ProjectAction.CopyPath, CopyPathCommand },
            { ProjectAction.CopyRelativePath, CopyRelativePathCommand },
            { ProjectAction.ShowInExplorer, ShowInExplorerCommand },
            { ProjectAction.Disconnect, DisconnectCommand },
            { ProjectAction.Close, CloseLocalCommand }
        };
    }

    [RelayCommand]
    private async Task NewFile()
    {
        if (SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = SelectedNodes[0];

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

        DialogResult result = await _dialogs.ShowDialogAsync(dialog);

        if (result == DialogResult.Ok)
        {
            await folderNode.NewFile(inputElement.Text);
        }
    }

    [RelayCommand]
    private async Task NewFolder()
    {
        if (SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = SelectedNodes[0];

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

        DialogResult result = await _dialogs.ShowDialogAsync(dialog);

        if (result == DialogResult.Ok)
        {
            await folderNode.NewFolder(inputElement.Text);
        }
    }

    [RelayCommand]
    private void CopyPath()
    {
        if (SelectedNodes.Count == 0)
        {
            return;
        }

        string paths = string.Join("\n", SelectedNodes.Select(n => n.Path));
        _clipboard.CopyTextAsync(paths);
    }

    [RelayCommand]
    private void CopyRelativePath()
    {
        if (SelectedNodes.Count == 0)
        {
            return;
        }

        if (FileSystem?.Root == null)
        {
            return;
        }

        string basePath = FileSystem.Root.Path;
        string paths = string.Join("\n",
            SelectedNodes.Select(n =>
                n.Path.StartsWith(basePath) ? n.Path[basePath.Length..].TrimStart('/', '\\') : n.Path));
        _clipboard.CopyTextAsync(paths);
    }

    [RelayCommand]
    private async Task Disconnect()
    {
        if (SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = SelectedNodes[0];

        if (FileSystem is SftpFileSystem && node.IsRoot)
        {
            await CloseFileSystemAsync();
        }
    }

    [RelayCommand]
    private async Task CloseLocal()
    {
        if (SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = SelectedNodes[0];

        if (FileSystem is LocalFileSystem && node.IsRoot)
        {
            await CloseFileSystemAsync();
        }
    }

    public async Task CloseFileSystemAsync()
    {
        if (_cts != null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        }

        if (FileSystem is SftpFileSystem sftp)
        {
            sftp.Disconnect();
        }

        RootItems.Clear();
        SelectedNodes.Clear();
        FileSystem = null;
        IsLoading = false;
    }

    private bool CanOpen()
    {
        return SelectedNodes.All(n => !n.IsRenaming);
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void Open()
    {
        foreach (FileSystemNode node in SelectedNodes)
        {
            node.OpenCommand?.Execute(null);
        }
    }

    [RelayCommand]
    private void Rename()
    {
        if (SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = SelectedNodes[0];

        if (!node.GetSupportedActions().Contains(ProjectAction.Rename))
        {
            return;
        }

        node.IsRenaming = true;

        node.Renamed += OnNodeOnRenamed;
        return;

        void OnNodeOnRenamed(object? __, NodeRenamedEventArgs args)
        {
            _ = Task.Run(async () =>
            {
                node.IsRenaming = false;

                if (args.OldName == args.NewName)
                {
                    return;
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(args.NewName))
                    {
                        Notification notification = Notification
                            .Create(TranslationKey.ProjectRenameFailedTitle)
                            .WithMessage(TranslationKey.ProjectRenameErrorNameEmpty)
                            .WithType(NotificationType.Error);

                        _notifs.Show(notification);

                        return;
                    }

                    await node.RenameAsync(args.NewName);
                }
                finally
                {
                    node.Renamed -= OnNodeOnRenamed;
                }
            });
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedNodes.Any(n => n.IsRoot))
        {
            return;
        }

        TranslatableString message = SelectedNodes.Count == 1
            ? new TranslatableString(TranslationKey.FileExplorerConfirmDeleteOneMessage, SelectedNodes[0].Name)
            : new TranslatableString(TranslationKey.FileExplorerConfirmDeleteMultipleMessage, SelectedNodes.Count);

        Dialog dialog = Dialog
            .Create(TranslationKey.FileExplorerConfirmDeleteTitle)
            .WithMessage(message)
            .WithIcon(Symbol.Delete)
            .WithButtons(new DialogButton(TranslationKey.ButtonDelete, DialogResult.Ok), DialogButton.Cancel);

        DialogResult result = await _dialogs.ShowDialogAsync(dialog);

        if (result != DialogResult.Ok)
        {
            return;
        }

        List<FileSystemNode> nodesToDelete = SelectedNodes.ToList();
        foreach (FileSystemNode node in nodesToDelete)
        {
            bool isSuccess = await node.DeleteAsync();

            if (isSuccess)
            {
                node.OnDeleted();
            }
        }
    }

    [RelayCommand]
    private void ShowInExplorer()
    {
        foreach (FileSystemNode node in SelectedNodes)
        {
            if (node.IsDirectory)
            {
                LinkOpener.OpenFolder(node.Path);
            }
            else
            {
                LinkOpener.OpenFolderWithFileSelection(node.Path);
            }
        }
    }

    private void UpdateContextMenuItems()
    {
        ContextMenuItems.Clear();

        List<FileSystemNode> selectedNodes = SelectedNodes.Where(n => n is not DummyFileNode).ToList();

        if (selectedNodes.Count == 0)
        {
            return;
        }

        IEnumerable<ProjectAction> actionsTemplate = selectedNodes.First().GetSupportedActions();
        HashSet<ProjectAction> commonActions = selectedNodes
            .Select(n => n.GetSupportedActions().Where(a => a != ProjectAction.Separator))
            .Aggregate((prev, next) => prev.Intersect(next))
            .ToHashSet();

        foreach (ProjectAction action in actionsTemplate)
        {
            if (action == ProjectAction.Separator)
            {
                ContextMenuItems.Add(ContextMenuItem.Separator);
                continue;
            }

            if (!commonActions.Contains(action))
            {
                continue;
            }

            ProjectMenuRegistry.Templates.TryGetValue(action, out MenuTemplate? template);
            if (template == null)
            {
                continue;
            }

            if (template.AllowOneItemOnly && selectedNodes.Count > 1)
            {
                continue;
            }

            _commandMap.TryGetValue(action, out IRelayCommand? command);
            ContextMenuItem item = ContextMenuItem.Item(template.Header)
                .WithIcon(template.Icon)
                .WithInputGesture(template.InputGestureText)
                .WithCommand(command);

            ContextMenuItems.Add(item);
        }

        List<ContextMenuItem> cleaned = CleanUpSeparators(ContextMenuItems.ToList());
        ContextMenuItems.Clear();
        ContextMenuItems.AddRange(cleaned);
    }

    private static List<ContextMenuItem> CleanUpSeparators(IEnumerable<ContextMenuItem> menuItems)
    {
        List<ContextMenuItem> cleaned = [];
        bool lastWasSeparator = true;

        foreach (ContextMenuItem item in menuItems)
        {
            if (item.IsSeparator)
            {
                if (lastWasSeparator)
                {
                    continue;
                }

                cleaned.Add(item);
                lastWasSeparator = true;
            }
            else
            {
                cleaned.Add(item);
                lastWasSeparator = false;
            }
        }

        if (cleaned.Count > 0 && cleaned[^1].IsSeparator)
        {
            cleaned.RemoveAt(cleaned.Count - 1);
        }

        return cleaned;
    }
}