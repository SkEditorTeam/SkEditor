using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Appearance.Themes;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.Filesystem.Sftp;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Projects.Models;
using SkEditor.Workspace.Remote.Messages;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Workspace.UI.Tree;

public partial class FileTreeViewModel : ObservableObject, IRecipient<RemoteConnectionStateChangedMessage>
{
    private readonly AppSettings _appSettings;
    private readonly IClipboardService _clipboard;
    private readonly IDialogService _dialogs;
    private readonly ILinkService _links;
    private readonly ILogService _logs;
    private readonly INotificationService _notifs;
    private readonly ITabService _tabs;
    private CancellationTokenSource? _cts;

    public ObservableCollection<ContextMenuItem> ContextMenuItems { get; } = [];

    [ObservableProperty]
    public partial IFileSystem? FileSystem { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public ObservableCollection<FileSystemNode> RootItems { get; } = [];

    public ObservableCollection<FileSystemNode> SelectedNodes { get; } = [];

    [ObservableProperty]
    public partial int ThemeVersion { get; set; }

    public FileTreeViewModel(ILogService logs, INotificationService notifs, IClipboardService clipboard,
        IDialogService dialogs, ILinkService links, ITabService tabs, AppSettings appSettings, IThemeService themes,
        IMessenger messenger)
    {
        _logs = logs;
        _notifs = notifs;
        _dialogs = dialogs;
        _tabs = tabs;
        _links = links;

        _clipboard = clipboard;
        _appSettings = appSettings;

        SelectedNodes.CollectionChanged += OnSelectedNodesChanged;

        InitializeCommandMap();

        messenger.Register(this);

        themes.ThemeChanged += () => Dispatcher.UIThread.Post(() => ThemeVersion++);
    }

    /// <summary>
    ///     Loads the file system and populates the root items.
    /// </summary>
    /// <returns>True if success</returns>
    public async Task<bool> LoadFileSystemAsync()
    {
        if (_cts != null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        }

        RootItems.Clear();
        IsLoading = true;

        if (FileSystem == null)
        {
            _logs.Warning("File system is null, cannot load!");
            IsLoading = false;
            return false;
        }

        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        CancellationTokenSource? thisCts = _cts;

        bool isSuccess;
        try
        {
            isSuccess = await FileSystem.LoadAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            if (ReferenceEquals(_cts, thisCts))
            {
                IsLoading = false;
            }

            return false;
        }

        if (!isSuccess)
        {
            if (ReferenceEquals(_cts, thisCts))
            {
                IsLoading = false;
            }

            return false;
        }

        if (!ReferenceEquals(_cts, thisCts))
        {
            return false;
        }

        if (FileSystem?.Root != null)
        {
            RootItems.Clear();
            RootItems.Add(FileSystem.Root);
            FileSystem.Root.IsExpanded = true;
        }

        IsLoading = false;
        return true;
    }

    public void OnSelectedNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateContextMenuItems();
    }

    public void HandleNodeClick(FileSystemNode node, bool isDoubleClick)
    {
        bool singleClickEnabled = _appSettings.IsExplorerSingleClickOpenEnabled;

        bool doubleClick = !singleClickEnabled && isDoubleClick;
        bool singleClick = singleClickEnabled && !isDoubleClick;

        if (doubleClick || singleClick)
        {
            node.OpenCommand?.Execute(null);
        }
    }

    public void Receive(RemoteConnectionStateChangedMessage message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (FileSystem is not SftpFileSystem sftpFileSystem || sftpFileSystem.Connection != message.Connection)
            {
                return;
            }

            switch (message.State)
            {
                case RemoteConnectionState.Reconnecting:
                    IsLoading = true;
                    break;

                case RemoteConnectionState.Connected:
                    IsLoading = false;
                    break;

                case RemoteConnectionState.Disconnected:
                    IsLoading = false;
                    RootItems.Clear();
                    SelectedNodes.Clear();
                    FileSystem = null;
                    break;
            }
        });
    }

    partial void OnFileSystemChanged(IFileSystem? value)
    {
        WeakReferenceMessenger.Default.Send(new FileSystemChangedMessage(this, value));
    }
}