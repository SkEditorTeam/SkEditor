using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Icons;

namespace SkEditor.Views.Controls.Sidebar;

public partial class RemoteSidebarPanel : UserControl
{
    public RemoteSidebarPanel()
    {
        InitializeComponent();
        
        NewRemoteConnectionButton.Command = new AsyncRelayCommand(async () =>
        {
            var window = new Windows.RemoteConnectionWindow();
            await window.ShowDialogOnMainWindow();
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