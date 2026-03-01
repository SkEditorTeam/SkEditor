using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Models.Remote;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Icons;
using SkEditor.Views.Windows;

namespace SkEditor.Views.Controls.Sidebar;

public partial class RemoteSidebarPanel : UserControl
{
    public RemoteSidebarPanel()
    {
        InitializeComponent();
        
        NewRemoteConnectionButton.Command = new AsyncRelayCommand(async () =>
        {
            var window = new RemoteConnectionWindow();
            RemoteConnectionInfo info = await window.ShowDialogOnMainWindow<RemoteConnectionInfo>();
            if (info == null) return;
            
            SkEditorAPI.Logs.Info($"Added new remote connection {info}");
            SkEditorAPI.Logs.Info($"Added new remote connection with name '{info.Name}': Host={info.Host}, Port={info.Port}, Username={info.Username}, AuthMethod={info.AuthMethod}");
        });
    }

    public class RemotePanel : SidebarPanel
    {
        public readonly RemoteSidebarPanel Panel = new();
        public override UserControl Content => Panel;
        public override IconSource Icon => Icons.GetPathIcon("RemoteDesktopIcon");

        public override IconSource IconActive => Icon;

        public override bool IsDisabled => false;
        public override int DesiredWidth => 300;
    }
}