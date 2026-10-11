using System.Collections.ObjectModel;
using System.ComponentModel;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Contracts.Services;

public interface ISidebarService
{
    ObservableCollection<ISidebarPanelViewModel> Panels { get; }
    ISidebarPanelViewModel? SelectedPanel { get; set; }
    string? SelectedPanelName { get; }

    void OpenPanel(string name);

    event PropertyChangedEventHandler? PropertyChanged;
}