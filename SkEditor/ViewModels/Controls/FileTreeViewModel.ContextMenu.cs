using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Models.Dialogs;
using SkEditor.Models.Dialogs.Elements;
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
            { ProjectAction.CopyPath, CopyPathCommand },
            { ProjectAction.CopyRelativePath, CopyRelativePathCommand },
            { ProjectAction.ShowInExplorer, ShowInExplorerCommand },
            { ProjectAction.Disconnect, DisconnectCommand },
        };
    }

    [RelayCommand]
    private async Task NewFile()
    {
        if (SelectedNodes.Count != 1) return;
        FileSystemNode node = SelectedNodes[0];

        if (node is not BaseFolderNode folderNode ||
            !folderNode.GetSupportedActions().Contains(ProjectAction.NewFile)) return;

        TextDialogElement inputElement = new()
        {
            PlaceholderText = "Enter file name",
            ValidationFunc = name =>
            {
                if (string.IsNullOrWhiteSpace(name)) return "Name cannot be empty";
                if (name.IndexOfAny(LocalFileNode.InvalidChars) >= 0) return "Name contains invalid characters";
                return folderNode.Children.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ? "A file or folder with this name already exists"
                    : null;
            }
        };

        Dialog dialog = Dialog
            .Create(TranslatableString.FromRawString("Create New File"),
                TranslatableString.FromRawString("Enter the name of the new file:"))
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
        if (SelectedNodes.Count != 1) return;
        FileSystemNode node = SelectedNodes[0];

        if (node is not BaseFolderNode folderNode ||
            !folderNode.GetSupportedActions().Contains(ProjectAction.NewFolder)) return;

        TextDialogElement inputElement = new()
        {
            PlaceholderText = "Enter folder name",
            ValidationFunc = name =>
            {
                if (string.IsNullOrWhiteSpace(name)) return "Name cannot be empty";
                if (name.IndexOfAny(LocalFileNode.InvalidChars) >= 0) return "Name contains invalid characters";
                return folderNode.Children.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ? "A file or folder with this name already exists"
                    : null;
            }
        };

        Dialog dialog = Dialog
            .Create(TranslatableString.FromRawString("Create New Folder"),
                TranslatableString.FromRawString("Enter the name of the new folder:"))
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
        if (SelectedNodes.Count == 0) return;

        string paths = string.Join("\n", SelectedNodes.Select(n => n.Path));
        _clipboard.SetTextAsync(paths);
    }

    [RelayCommand]
    private void CopyRelativePath()
    {
        if (SelectedNodes.Count == 0) return;
        if (FileSystem?.Root == null) return;

        string basePath = FileSystem.Root.Path;
        string paths = string.Join("\n",
            SelectedNodes.Select(n =>
                n.Path.StartsWith(basePath) ? n.Path[basePath.Length..].TrimStart('/', '\\') : n.Path));
        _clipboard.SetTextAsync(paths);
    }

    [RelayCommand]
    private void Disconnect()
    {
        if (SelectedNodes.Count != 1) return;
        FileSystemNode node = SelectedNodes[0];

        if (FileSystem is SftpFileSystem && node.IsRoot)
        {
            Dispatcher.UIThread.Post(() => { _ = CloseFileSystemAsync(); });
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

    private bool CanOpen() => SelectedNodes.All(n => !n.IsRenaming);

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void Open()
    {
        foreach (var node in SelectedNodes)
            node.OpenCommand?.Execute(null);
    }

    [RelayCommand]
    private void Rename()
    {
        if (SelectedNodes.Count != 1) return;
        FileSystemNode node = SelectedNodes[0];

        if (!node.GetSupportedActions().Contains(ProjectAction.Rename)) return;

        node.IsRenaming = true;

        node.Renamed += OnNodeOnRenamed;
        return;

        void OnNodeOnRenamed(object? __, NodeRenamedEventArgs args)
        {
            _ = Task.Run(async () =>
            {
                node.IsRenaming = false;

                try
                {
                    if (string.IsNullOrWhiteSpace(args.NewName))
                    {
                        _notifs.ShowError(TranslationKey.ProjectRenameFailedTitle,
                            TranslationKey.ProjectRenameErrorNameEmpty);

                        return;
                    }

                    await node.Rename(args.NewName);
                }
                finally
                {
                    node.Renamed -= OnNodeOnRenamed;
                }
            });
        }
    }

    [RelayCommand]
    private void ShowInExplorer()
    {
        foreach (var node in SelectedNodes)
        {
            if (node.IsDirectory) LinkOpener.OpenFolder(node.Path);
            else LinkOpener.OpenFolderWithFileSelection(node.Path);
        }
    }

    private void UpdateContextMenuItems()
    {
        ContextMenuItems.Clear();
        if (SelectedNodes.Count == 0) return;

        var actionsTemplate = SelectedNodes.First().GetSupportedActions();
        var commonActions = SelectedNodes
            .Select(n => n.GetSupportedActions().Where(a => a != ProjectAction.Separator))
            .Aggregate((prev, next) => prev.Intersect(next))
            .ToHashSet();

        foreach (var action in actionsTemplate)
        {
            if (action == ProjectAction.Separator)
            {
                ContextMenuItems.Add(ContextMenuItem.Separator);
                continue;
            }

            if (!commonActions.Contains(action)) continue;

            var template = ProjectMenuRegistry.Templates[action];

            if (template.AllowOneItemOnly && SelectedNodes.Count > 1) continue;

            _commandMap.TryGetValue(action, out IRelayCommand? command);
            var item = ContextMenuItem.Item(template.Header)
                .WithIcon(template.Icon)
                .WithInputGesture(template.InputGestureText)
                .WithCommand(command);

            ContextMenuItems.Add(item);
        }

        var cleaned = CleanUpSeparators(ContextMenuItems.ToList());
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