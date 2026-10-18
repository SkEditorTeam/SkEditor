using System;
using FluentIcons.Common;
using SkEditor.Configuration.UI;
using SkEditor.Layout.Menu.Models;
using SkEditor.Workspace.Tabs;

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