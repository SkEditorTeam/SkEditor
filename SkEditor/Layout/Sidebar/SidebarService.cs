using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Layout.Sidebar.Messages;
using SkEditor.Layout.Sidebar.UI;

namespace SkEditor.Layout.Sidebar;

public partial class SidebarService : ObservableObject, ISidebarService, IRecipient<RequestPaneOpenMessage>
{
    public SidebarService(IEnumerable<ISidebarPanelViewModel> panels, IMessenger messenger)
    {
        Panels = [.. panels];
        messenger.Register(this);
    }

    public ObservableCollection<ISidebarPanelViewModel> Panels { get; }

    [ObservableProperty]
    public partial ISidebarPanelViewModel? SelectedPanel { get; set; }

    public string? SelectedPanelName => SelectedPanel?.Name?.Value;

    public void OpenPanel(string name)
    {
        SelectedPanel = Panels.FirstOrDefault(p => p.Name.Value == name);
    }

    public void Receive(RequestPaneOpenMessage message)
    {
        OpenPanel(message.PaneName);
    }
}