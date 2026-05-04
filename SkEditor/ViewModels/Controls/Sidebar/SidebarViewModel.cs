using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.ViewModels.Controls;

public partial class SidebarViewModel(IEnumerable<ISidebarPanelViewModel> panels) : ObservableObject
{
    public ObservableCollection<ISidebarPanelViewModel> Items { get; } = new(panels);

    [ObservableProperty]
    public partial ISidebarPanelViewModel? SelectedItem { get; set; }
}