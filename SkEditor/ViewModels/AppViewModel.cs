using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services.Files;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.ViewModels;

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