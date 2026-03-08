using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Icons;
using SkEditor.ViewModels.Controls;
using SkEditor.Views.Windows;

namespace SkEditor.Views.Controls.Sidebar;

public partial class RemoteSidebarPanel : UserControl
{
    private readonly IRemoteConnectionService _remoteConnectionService;

    public RemoteSidebarPanel()
    {
        InitializeComponent();
    }

    [SetsRequiredMembers]
    public RemoteSidebarPanel(FileTreeViewModel fileTree, IRemoteConnectionService remoteConnectionService) : this()
    {
        FileTree = fileTree;
        _remoteConnectionService = remoteConnectionService;

        DataContext = this;
    }

    public required FileTreeViewModel FileTree { get; init; }

    [RelayCommand]
    private async Task NewRemoteConnection()
    {
        RemoteConnectionWindow window = App.Services.GetRequiredService<RemoteConnectionWindow>();
        RemoteConnectionInfo info = await window.ShowDialogOnMainWindow<RemoteConnectionInfo>();
        if (info == null)
        {
            return;
        }

        RemoteConnection connection = new(info);

        if (connection.AuthMethod == RemoteAuthMethod.Password)
        {
            _remoteConnectionService.SavePassword(connection, info.Password);
        }

        SkEditorAPI.Configs.GetConnectionsConfig().Connections.Add(connection);
        _ = SkEditorAPI.Configs.GetConnectionsConfig().Save();

        FileTree.AddConnection(connection);
    }

    public class RemotePanel(FileTreeViewModel fileTreeViewModel, IRemoteConnectionService remoteConnectionService)
        : SidebarPanel
    {
        public readonly RemoteSidebarPanel Panel = new(fileTreeViewModel, remoteConnectionService);
        public override UserControl Content => Panel;
        public override IconSource Icon => Icons.GetPathIcon("RemoteDesktopIcon");

        public override IconSource IconActive => Icon;

        public override bool IsDisabled => false;
        public override int DesiredWidth => 300;
    }
}