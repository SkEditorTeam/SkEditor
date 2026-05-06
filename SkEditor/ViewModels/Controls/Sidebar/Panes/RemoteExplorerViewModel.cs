using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Remote;
using SkEditor.Models.UI;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public partial class RemoteExplorerViewModel(
    FileTreeViewModel fileTree,
    ILogService logs,
    INotificationService notifications,
    IRemoteConnectionService remoteConnectionService,
    WindowRegistry windowRegistry,
    ConnectionsConfig connectionsConfig) : ObservableObject, ISidebarPanelViewModel
{
    [ObservableProperty]
    public partial RemoteConnection? SelectedConnection { get; set; }

    public required FileTreeViewModel FileTree { get; init; } = fileTree;

    public ObservableCollection<RemoteConnection> RemoteConnections { get; } = new(connectionsConfig.Connections);

    public string Name => "Remote Explorer";
    public StandardIcon Icon => "RemoteDesktopIcon";

    partial void OnSelectedConnectionChanged(RemoteConnection? value)
    {
        if (value != null)
        {
            _ = HandleConnectionChangedAsync(value);
        }
    }

    private async Task HandleConnectionChangedAsync(RemoteConnection value)
    {
        bool success = await ChangeConnection(value);
        if (!success)
        {
            SelectedConnection = null;
        }
    }
    
    private async Task<bool> ChangeConnection(RemoteConnection connection)
    {
        if (connection == null)
        {
            logs.Warning("Selected connection is null!");
            return false;
        }
        
        FileTree.FileSystem = new SftpFileSystem(connection, remoteConnectionService, notifications, logs);

        bool success = await FileTree.LoadFileSystemAsync();
        return success;
    }

    [RelayCommand]
    private async Task NewRemoteConnection()
    {
        RemoteConnectionWindow window = App.Services.GetRequiredService<RemoteConnectionWindow>();

        if (windowRegistry.Active is not { } mainWindow)
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
            remoteConnectionService.SavePassword(connection, info.Password);
        }

        connectionsConfig.Connections.Add(connection);
        _ = connectionsConfig.Save();

        RemoteConnections.Add(connection);
    }
}