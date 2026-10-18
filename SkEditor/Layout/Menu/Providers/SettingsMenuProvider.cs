using System;
using FluentIcons.Common;
using SkEditor.Workspace.Tabs;
using MainMenuItem = SkEditor.Layout.Menu.Models.MainMenuItem;
using SettingsTabViewModel = SkEditor.Configuration.UI.SettingsTabViewModel;

namespace SkEditor.Layout.Menu.Providers;

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