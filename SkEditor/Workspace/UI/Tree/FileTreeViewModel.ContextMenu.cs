using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Workspace.Filesystem;
using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.Filesystem.Sftp;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Projects.Models;

namespace SkEditor.Workspace.UI.Tree;

public partial class FileTreeViewModel
{
    private FileTreeCommandHandler _commandHandler;
    private Dictionary<ProjectAction, IRelayCommand> _commandMap;

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
        WeakReferenceMessenger.Default.Send(new ProjectChangedMessage(null));
    }

    public async Task<bool> Disconnect()
    {
        return await _commandHandler.Disconnect();
    }

    [MemberNotNull(nameof(_commandMap), nameof(_commandHandler))]
    private void InitializeCommandMap()
    {
        _commandHandler =
            new FileTreeCommandHandler(this, _appSettings, _dialogs, _clipboard, _notifs, _logs, _tabs, _links);

        _commandMap = new Dictionary<ProjectAction, IRelayCommand>
        {
            { ProjectAction.Open, FromAction(_commandHandler.Open) },
            { ProjectAction.NewFile, FromAction(_commandHandler.NewFile) },
            { ProjectAction.NewFolder, FromAction(_commandHandler.NewFolder) },
            { ProjectAction.Rename, FromAction(_commandHandler.Rename) },
            { ProjectAction.Delete, FromAction(_commandHandler.Delete) },
            { ProjectAction.CopyPath, FromAction(_commandHandler.CopyPath) },
            { ProjectAction.CopyRelativePath, FromAction(_commandHandler.CopyRelativePath) },
            { ProjectAction.ShowInExplorer, FromAction(_commandHandler.ShowInExplorer) },
            { ProjectAction.Refresh, FromAction(_commandHandler.Refresh) },
            { ProjectAction.Disconnect, FromAction(DisconnectWithChecks) },
            { ProjectAction.Close, FromAction(_commandHandler.CloseLocal) }
        };
    }

    private static RelayCommand FromAction(Action action)
    {
        return new RelayCommand(action);
    }

    private static AsyncRelayCommand FromAction(Func<Task> action)
    {
        return new AsyncRelayCommand(action);
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

        List<ContextMenuItem> cleaned = ContextMenuItem.CleanUpSeparators(ContextMenuItems.ToList());
        ContextMenuItems.Clear();
        ContextMenuItems.AddRange(cleaned);
    }

    [RelayCommand(CanExecute = nameof(IsNotRenaming))]
    private void Rename()
    {
        _commandHandler.Rename();
    }

    [RelayCommand(CanExecute = nameof(IsNotRenaming))]
    private async Task Delete()
    {
        await _commandHandler.Delete();
    }

    [RelayCommand(CanExecute = nameof(IsNotRenaming))]
    private void Open()
    {
        _commandHandler.Open();
    }

    private bool IsNotRenaming()
    {
        return !SelectedNodes.Any(n => n.IsRenaming);
    }

    [RelayCommand]
    private async Task NewFile()
    {
        await _commandHandler.NewFile();
    }

    [RelayCommand]
    private async Task NewFolder()
    {
        await _commandHandler.NewFolder();
    }

    private void DisconnectWithChecks()
    {
        if (SelectedNodes.Count != 1)
        {
            return;
        }

        FileSystemNode node = SelectedNodes[0];
        if (!node.IsRoot)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => { _ = Disconnect(); });
    }
}