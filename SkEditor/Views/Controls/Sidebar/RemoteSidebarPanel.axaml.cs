using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using GitCredentialManager;
using SkEditor.API;
using SkEditor.Models.Remote;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Icons;
using SkEditor.Views.Windows;

namespace SkEditor.Views.Controls.Sidebar;

public partial class RemoteSidebarPanel : UserControl
{
    const string CredentialTarget = "skeditor://sftp-connections";
    
    public RemoteSidebarPanel()
    {
        InitializeComponent();

        NewRemoteConnectionButton.Command = new AsyncRelayCommand(async () =>
        {
            var window = new RemoteConnectionWindow();
            RemoteConnectionInfo info = await window.ShowDialogOnMainWindow<RemoteConnectionInfo>();
            if (info == null) return;

            RemoteConnection connection = new(info);

            if (connection.AuthMethod == RemoteAuthMethod.Password)
            {
                ICredentialStore store = CredentialManager.Create("SkEditor Remote Connections");
                store.AddOrUpdate(CredentialTarget, connection.Id.ToString(), info.Password);

                ICredential? cred = store.Get(CredentialTarget, connection.Id.ToString());
                if (cred != null)
                {
                    SkEditorAPI.Logs.Info($"Hasło dla {connection.Name} siedzi bezpiecznie w sejfie: {cred.Password}");
                }
            }

            SkEditorAPI.Configs.GetConnectionsConfig().Connections.Add(connection);
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