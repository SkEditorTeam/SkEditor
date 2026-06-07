using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Remote;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls.FileTree;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public partial class RemoteExplorerViewModel : ObservableObject, ISidebarPanelViewModel,
    IRecipient<FileSystemChangedMessage>
{
    private readonly INotificationService _notifications;
    private readonly IRemoteConnectionService _remoteConnections;
    private readonly IWindowDialogService _windowDialogs;

    private int _connectionChangeId;
    private bool _isChangingConnection;

    public RemoteExplorerViewModel(FileTreeViewModel fileTree,
        INotificationService notifications,
        IRemoteConnectionService remoteConnections,
        IWindowDialogService windowDialogs,
        ConnectionsConfig connectionsConfig,
        IMessenger messenger)
    {
        _notifications = notifications;
        _remoteConnections = remoteConnections;
        _windowDialogs = windowDialogs;
        FileTree = fileTree;
        RemoteConnections = connectionsConfig.Connections;

        messenger.Register(this);
    }
    
    [ObservableProperty]
    public partial RemoteConnection? SelectedConnection { get; set; }

    public RemoteConnection? CurrentConnection { get; set; }

    public required FileTreeViewModel FileTree { get; init; }

    public ObservableCollection<RemoteConnection> RemoteConnections { get; }

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

    public TranslatableString Name => TranslationKey.RemoteExplorerTitle;
    public StandardIcon Icon => "RemoteDesktopIcon";

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Connect()
    {
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

            FileTree.FileSystem = new SftpFileSystem(target, _remoteConnections, _notifications);
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