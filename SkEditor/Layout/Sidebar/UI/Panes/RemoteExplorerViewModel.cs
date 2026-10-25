using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using R3;
using SkEditor.Appearance.Icons;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Localization;
using SkEditor.Core.Notifications;
using SkEditor.Windowing;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Filesystem.Sftp;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Remote;
using SkEditor.Workspace.Remote.Models;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;
using SkEditor.Workspace.UI.Tree;

namespace SkEditor.Layout.Sidebar.UI.Panes;

public partial class RemoteExplorerViewModel : ObservableObject, ISidebarPanelViewModel,
    IRecipient<FileSystemChangedMessage>
{
    private readonly IDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly IRemoteConnectionService _remoteConnections;
    private readonly ITabService _tabs;
    private readonly IWindowDialogService _windowDialogs;
    private readonly ConnectionsConfig _connectionsConfig;

    private int _connectionChangeId;

    private bool _ignoreNextConnectionChange;
    private bool _isChangingConnection;

    public RemoteConnection? CurrentConnection { get; set; }

    public required FileTreeViewModel FileTree { get; init; }
    public StandardIcon Icon => "RemoteDesktopIcon";

    public TranslatableString Name => TranslationKey.RemoteExplorerTitle;

    public ObservableCollection<RemoteConnection> RemoteConnections => _connectionsConfig.Connections;

    [ObservableProperty]
    public partial RemoteConnection? SelectedConnection { get; set; }

    public RemoteExplorerViewModel(FileTreeViewModel fileTree,
        INotificationService notifications,
        IRemoteConnectionService remoteConnections,
        IWindowDialogService windowDialogs,
        ITabService tabs,
        IDialogService dialogs,
        ConnectionsConfig connectionsConfig,
        IMessenger messenger)
    {
        _notifications = notifications;
        _remoteConnections = remoteConnections;
        _windowDialogs = windowDialogs;
        _tabs = tabs;
        _dialogs = dialogs;
        _connectionsConfig = connectionsConfig;

        FileTree = fileTree;
        connectionsConfig
            .ObservePropertyChanged(x => x.Connections)
            .Subscribe(_ => OnPropertyChanged(nameof(RemoteConnections)));

        messenger.Register(this);
    }

    public void Receive(FileSystemChangedMessage message)
    {
        if (message.Sender != FileTree || _isChangingConnection)
        {
            return;
        }

        if (message.NewFileSystem != null)
        {
            return;
        }

        CurrentConnection = null;
        SelectedConnection = null;
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Connect()
    {
        if (_ignoreNextConnectionChange)
        {
            _ignoreNextConnectionChange = false;
            return;
        }

        if (CurrentConnection is not null && _tabs.AllTabs.OfType<IFileTab>().Any(t => t.File is SftpFile))
        {
            Dialog dialog = Dialog
                .Create(TranslationKey.RemoteConnectionDisconnectTitle)
                .WithMessage(TranslationKey.RemoteConnectionDisconnectMessage)
                .WithIcon(Symbol.PlugDisconnected)
                .WithButtons(new DialogButton(TranslationKey.ButtonDisconnect, DialogResult.Ok), DialogButton.Cancel);

            DialogResult result = await _dialogs.ShowDialogAsync(dialog);
            if (result != DialogResult.Ok)
            {
                _ignoreNextConnectionChange = true;
                SelectedConnection = CurrentConnection;
                return;
            }
        }

        RemoteConnection? targetedConnection = SelectedConnection;
        if (targetedConnection == null)
        {
            return;
        }

        bool success = await ChangeConnection(targetedConnection);

        if (SelectedConnection != targetedConnection)
        {
            return;
        }

        if (success)
        {
            CurrentConnection = SelectedConnection;
        }
        else
        {
            CurrentConnection = null;
            SelectedConnection = null;
        }
    }

    private async Task<bool> ChangeConnection(RemoteConnection target)
    {
        int currentId = ++_connectionChangeId;
        _isChangingConnection = true;

        try
        {
            if (FileTree.FileSystem != null)
            {
                await FileTree.CloseFileSystemAsync();

                if (_connectionChangeId != currentId)
                {
                    return false;
                }

                FileTree.FileSystem = null;
            }

            FileTree.FileSystem = new SftpNodeFileSystem(target, _remoteConnections, _notifications);
            bool success = await FileTree.LoadFileSystemAsync();

            return success;
        }
        finally
        {
            if (_connectionChangeId == currentId)
            {
                _isChangingConnection = false;
            }
        }
    }

    [RelayCommand]
    private async Task NewRemoteConnection()
    {
        await _windowDialogs.ShowManageRemoteConnectionsDialogAsync();
    }
}