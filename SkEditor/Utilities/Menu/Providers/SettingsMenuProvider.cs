using System;
using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class SettingsMenuProvider(ITabService tabs, Func<SettingsTab> settingsTabFactory) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        return MainMenuItem.IconOnly(Symbol.Settings).WithCommand(() => tabs.OpenUniqueTab(settingsTabFactory));
    }
}