using Avalonia.Controls;
using RemoteSidebarViewModel = SkEditor.ViewModels.Controls.Sidebar.Panes.RemoteSidebarViewModel;

namespace SkEditor.Views.Controls.Sidebar;

public partial class RemoteSidebarPanel : UserControl
{
    public RemoteSidebarPanel() => InitializeComponent();

    public RemoteSidebarPanel(RemoteSidebarViewModel viewModel) : this() => DataContext = viewModel;
}