using System.Collections.ObjectModel;
using System.ComponentModel;
using SkEditor.Layout.Sidebar.UI;

namespace SkEditor.Layout.Sidebar;

public interface ISidebarService
{
    ObservableCollection<ISidebarPanelViewModel> Panels { get; }
    ISidebarPanelViewModel? SelectedPanel { get; set; }
    string? SelectedPanelName { get; }

    void OpenPanel(string name);

    event PropertyChangedEventHandler? PropertyChanged;
}