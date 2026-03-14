using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Icons;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Views.Controls.Sidebar;

public partial class RemoteSidebarPanel : UserControl
{
    public RemoteSidebarPanel() => InitializeComponent();

    public RemoteSidebarPanel(RemoteSidebarViewModel viewModel) : this() => DataContext = viewModel;

    public class RemotePanel(RemoteSidebarViewModel viewModel) : SidebarPanel
    {
        public readonly RemoteSidebarPanel Panel = new(viewModel);
        public override UserControl Content => Panel;
        public override IconSource Icon => Icons.GetPathIcon("RemoteDesktopIcon");

        public override IconSource IconActive => Icon;

        public override bool IsDisabled => false;
        public override int DesiredWidth => 300;
    }
}