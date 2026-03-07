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
    public RemoteSidebarPanel(FileTreeViewModel fileTree, IRemoteConnectionService remoteConnectionService)
    {
        FileTree = fileTree;
        InitializeComponent();

        NewRemoteConnectionButton.Command = new AsyncRelayCommand(async () =>
        {
            var window = App.Services.GetRequiredService<RemoteConnectionWindow>();
            RemoteConnectionInfo info = await window.ShowDialogOnMainWindow<RemoteConnectionInfo>();
            if (info == null) return;

            RemoteConnection connection = new(info);

            if (connection.AuthMethod == RemoteAuthMethod.Password)
            {
                remoteConnectionService.SavePassword(connection, info.Password);
            }

            SkEditorAPI.Configs.GetConnectionsConfig().Connections.Add(connection);
        });
    }

    public FileTreeViewModel FileTree { get; }

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