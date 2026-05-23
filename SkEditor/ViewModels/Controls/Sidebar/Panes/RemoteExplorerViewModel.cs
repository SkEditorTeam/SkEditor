using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Remote;
using SkEditor.Models.UI;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public partial class RemoteExplorerViewModel : ObservableObject, ISidebarPanelViewModel,
    IRecipient<FileSystemChangedMessage>
{
    private readonly ConnectionsConfig _connectionsConfig;
    private readonly INotificationService _notifications;
    private readonly IRemoteConnectionService _remoteConnections;
    private readonly WindowRegistry _windowRegistry;

    private int _connectionChangeId;
    private bool _isChangingConnection;

    public RemoteExplorerViewModel(FileTreeViewModel fileTree,
        INotificationService notifications,
        IRemoteConnectionService remoteConnections,
        WindowRegistry windowRegistry,
        ConnectionsConfig connectionsConfig,
        IMessenger messenger)
    {
        _notifications = notifications;
        _remoteConnections = remoteConnections;
        _windowRegistry = windowRegistry;
        _connectionsConfig = connectionsConfig;
        FileTree = fileTree;
        RemoteConnections = new ObservableCollection<RemoteConnection>(connectionsConfig.Connections);

        messenger.Register(this);
    }

    [ObservableProperty]
    public partial RemoteConnection? SelectedConnection { get; set; }

    public required FileTreeViewModel FileTree { get; init; }

    public ObservableCollection<RemoteConnection> RemoteConnections { get; }

    public void Receive(FileSystemChangedMessage message)
    {
        if (message.Sender != FileTree || _isChangingConnection)
        {
            return;
        }

        if (message.NewFileSystem == null)
        {
            SelectedConnection = null;
        }
    }

    public TranslatableString Name => TranslationKey.RemoteExplorerTitle;
    public StandardIcon Icon => "RemoteDesktopIcon";

    [RelayCommand]
    private async Task Connect(RemoteConnection? connection)
    {
        if (connection == null)
        {
            return;
        }

        bool success = await ChangeConnection(connection);
        if (!success)
        {
            SelectedConnection = null;
        }
    }

    private async Task<bool> ChangeConnection(RemoteConnection connection)
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

            FileTree.FileSystem = new SftpFileSystem(connection, _remoteConnections, _notifications);
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
        RemoteConnectionWindow window = App.Services.GetRequiredService<RemoteConnectionWindow>();

        if (_windowRegistry.Active is not { } mainWindow)
        {
            return;
        }

        RemoteConnectionInfo? info = await window.ShowDialog<RemoteConnectionInfo>(mainWindow);
        if (info == null)
        {
            return;
        }

        RemoteConnection connection = new(info);

        if (connection.AuthMethod == RemoteAuthMethod.Password)
        {
            _remoteConnections.SavePassword(connection, info.Password);
        }

        _connectionsConfig.Connections.Add(connection);
        _ = _connectionsConfig.Save();

        RemoteConnections.Add(connection);
    }
}