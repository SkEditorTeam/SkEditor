using Avalonia.Controls;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Views.Controls.Sidebar;

public partial class RemoteSidebarPanel : UserControl
{
    public RemoteSidebarPanel() => InitializeComponent();

    public RemoteSidebarPanel(RemoteSidebarViewModel viewModel) : this() => DataContext = viewModel;
}