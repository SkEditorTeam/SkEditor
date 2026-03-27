using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Extensions;
using SkEditor.Views.Windows;

namespace SkEditor.ViewModels.Controls;

public partial class RemoteSidebarViewModel(
    FileTreeViewModel fileTree,
    IRemoteConnectionService remoteConnectionService) : ObservableObject
{
    [ObservableProperty]
    private RemoteConnection? _selectedConnection;

    public required FileTreeViewModel FileTree { get; init; } = fileTree;

    // public ObservableCollection<RemoteConnection> RemoteConnections { get; } =
    //     new(SkEditorAPI.Configs.GetConnectionsConfig().Connections);
    public ObservableCollection<RemoteConnection> RemoteConnections => [];

    public bool NoSelectedConnection => SelectedConnection == null;

    partial void OnSelectedConnectionChanged(RemoteConnection? value)
    {
        if (value != null)
        {
            _ = HandleConnectionChangedAsync(value);
        }

        OnPropertyChanged(nameof(NoSelectedConnection));
    }

    private async Task HandleConnectionChangedAsync(RemoteConnection value)
    {
        bool success = await FileTree.ChangeConnection(value);
        if (!success)
        {
            SelectedConnection = null;
        }
    }

    [RelayCommand]
    private async Task NewRemoteConnection()
    {
        RemoteConnectionWindow window = App.Services.GetRequiredService<RemoteConnectionWindow>();
        RemoteConnectionInfo? info = await window.ShowDialogOnMainWindow<RemoteConnectionInfo>();
        if (info == null)
        {
            return;
        }

        RemoteConnection connection = new(info);

        if (connection.AuthMethod == RemoteAuthMethod.Password)
        {
            remoteConnectionService.SavePassword(connection, info.Password);
        }

        // SkEditorAPI.Configs.GetConnectionsConfig().Connections.Add(connection);
        // _ = SkEditorAPI.Configs.GetConnectionsConfig().Save();

        RemoteConnections.Add(connection);
    }
}