using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Models.UI;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public class ExplorerViewModel : ObservableObject, ISidebarPanelViewModel
{
    public string Name => "Explorer";
    public StandardIcon Icon => Symbol.Folder;
}