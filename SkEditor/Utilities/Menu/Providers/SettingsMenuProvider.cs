using System;
using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;
using SettingsTabViewModel = SkEditor.ViewModels.Tabs.SettingsTabViewModel;

namespace SkEditor.Utilities.Menu.Providers;

public class SettingsMenuProvider(ITabService tabs, Func<SettingsTabViewModel> settingsTabFactory) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        return MainMenuItem
            .IconOnly(Symbol.Settings)
            .WithInputGesture("Ctrl+,")
            .WithCommand(() => tabs.OpenUniqueTab(settingsTabFactory));
    }
}