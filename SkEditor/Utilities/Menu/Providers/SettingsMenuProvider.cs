using System;
using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Menu;
using SkEditor.Models.Tabs;

namespace SkEditor.Utilities.Menu.Providers;

public class SettingsMenuProvider(ITabService tabs, Func<SettingsTab> settingsTabFactory) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        return MainMenuItem.IconOnly(Symbol.Settings).WithCommand(() => tabs.OpenUniqueTab(settingsTabFactory));
    }
}