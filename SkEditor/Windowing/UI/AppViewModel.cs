using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Workspace.Tabs;
using SettingsTabViewModel = SkEditor.Configuration.UI.SettingsTabViewModel;

namespace SkEditor.Windowing.UI;

public partial class AppViewModel(
    ITabService tabs,
    Func<SettingsTabViewModel> settingsTabFactory) : ObservableObject
{
    [RelayCommand]
    private void OpenSettings()
    {
        tabs.OpenUniqueTab(settingsTabFactory);
    }
}