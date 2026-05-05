using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Models.UI;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public class RemoteExplorerViewModel : ObservableObject, ISidebarPanelViewModel
{
    public string Name => "Remote Explorer";
    public StandardIcon Icon => "RemoteDesktopIcon";
}